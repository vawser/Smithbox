use core::slice;
use num_bigint::BigUint;
use pkcs1::{RsaPublicKey, der::{Decode}};
use rayon::prelude::*;
use std::{
    ffi::{CString},
    os::raw::{c_char, c_int},
    ptr,
};

/// Error codes for the C ABI
#[repr(C)]
#[allow(non_camel_case_types)]
pub enum Bhderr {
    BHD_OK = 0,
    BHD_ERR_INVALID_ARG = 1,
    BHD_ERR_KEY_PARSE = 2,
    BHD_ERR_UTF8 = 3,
    BHD_ERR_OUTPUT_TOO_SMALL = 4,
    BHD_ERR_INTERNAL = 5,
}

/// Clears any existing error message in the parameter.
fn clear_err(err: *mut *mut c_char) {
    if !err.is_null() {
        unsafe { *err = ptr::null_mut() };
    }
}

/// Sets the error message in the parameter.
fn set_err(err: *mut *mut c_char, msg: &str) {
    if err.is_null() {
        return;
    }
    let c = CString::new(msg).unwrap_or_else(|_| CString::new("Error").unwrap());
    unsafe { *err = c.into_raw() };
}

/// Frees an error string previously returned by this library.
///
/// # Safety
///
/// - `err_msg` must either be:
///   - a null pointer, or
///   - a pointer previously returned via an `err_msg` out-parameter
///     by this library (e.g. from `get_decrypted_size`, or `decrypt`).
/// - `err_msg` must not be freed more than once.
/// - `err_msg` must not be modified by the caller before being freed.
///
/// Passing any other pointer results in undefined behavior.
#[no_mangle]
pub unsafe extern "C" fn free_error(err_msg: *mut c_char) {
    if err_msg.is_null() {
        return;
    }
    drop(CString::from_raw(err_msg));
}

/// Information needed to perform RSA decryption.
pub struct RsaKey {
    modulus: BigUint,
    exp: BigUint,
    /// Input block size in bytes
    in_s: usize,
    /// Output block size in bytes
    out_s: usize,
}

/// Computes RSA block sizings from the modulus.
fn compute_sizes(modulus: &BigUint) -> Result<(u64, u64), String> {
    let bits = modulus.bits();
    if bits == 0 {
        return Err("Invalid RSA modulus (0 bits)".to_owned());
    }
    if bits <= 1 {
        return Err("Invalid RSA modulus (<= 1 bit)".to_owned());
    }
    let in_s = (bits + 7) / 8;
    let out_s = (bits - 1) / 8;
    if in_s == 0 || out_s == 0 {
        return Err("Invalid RSA modulus sizing".to_owned());
    }
    Ok((in_s, out_s))
}

/// Parses a PEM PKCS#1 public key from raw bytes.
fn parse_key_from_bytes(key_pem: &[u8]) -> Result<RsaKey, String> {
    let (_, pem_decoded) = pkcs1::der::pem::decode_vec(key_pem).map_err(|e| format!("Failed to parse PKCS#1 RSA public key: {e}"))?;
    
    let decoded = RsaPublicKey::from_der(&pem_decoded)
        .map_err(|e| format!("Failed to parse PKCS#1 RSA public key: {e}"))?;

    let modulus = BigUint::from_bytes_be(decoded.modulus.as_bytes());
    let exp = BigUint::from_bytes_be(decoded.public_exponent.as_bytes());
    let (in_s, out_s) = compute_sizes(&modulus)?;

    Ok(RsaKey {
        modulus,
        exp,
        in_s: in_s as usize,
        out_s: out_s as usize,
    })
}

/// Computes the exact number of bytes required to hold the decrypted output of
/// an encrypted document of size `encrypted_len`.
///
/// On success, `*out_decrypted_len` is set to the required size in bytes.
/// 
/// On failure, `*out_decrypted_len` is not modified, and `*err_msg` (if
/// non-null) receives an allocated error string which must be freed with
/// `free_error`.
/// 
/// # Safety
///
/// - `key_pem` must be a valid pointer.
/// - `key_pem_len` must be the number of bytes readable at `key_pem`.
/// - `out_decrypted_len` must be a valid, writable pointer to `usize`.
/// - `err_msg` must be either null or a valid, writable pointer to a
///   `*mut c_char`.
///
/// Passing invalid pointers results in undefined behavior.
#[no_mangle]
pub unsafe extern "C" fn get_decrypted_size(
    key_pem: *const u8,
    key_pem_len: usize,
    encrypted_len: usize,
    out_decrypted_len: *mut usize,
    err_msg: *mut *mut c_char,
) -> c_int {
    clear_err(err_msg);

    if key_pem_len > 0 && key_pem.is_null() {
        set_err(err_msg, "key_pem is null");
        return Bhderr::BHD_ERR_INVALID_ARG as c_int;
    }

    let key_bytes = slice::from_raw_parts(key_pem, key_pem_len);

    let key = match parse_key_from_bytes(key_bytes) {
        Ok(k) => k,
        Err(e) => {
            set_err(err_msg, &e);
            return Bhderr::BHD_ERR_KEY_PARSE as c_int;
        }
    };

    let blocks = if encrypted_len == 0 {
        0
    } else {
        encrypted_len.div_ceil(key.in_s)
    };

    // exact output size, no padding or terminator
    let required = blocks.saturating_mul(key.out_s);
    unsafe { *out_decrypted_len = required };
    Bhderr::BHD_OK as c_int
}

/// Decrypts a single RSA block using raw modular exponentiation.
fn decrypt_block(in_c: &[u8], out_c: &mut [u8], key: &RsaKey) -> Result<(), String> {
    debug_assert_eq!(out_c.len(), key.out_s);

    let c = BigUint::from_bytes_be(in_c);
    let m = c.modpow(&key.exp, &key.modulus);
    let bytes = m.to_bytes_be();

    if bytes.len() > key.out_s {
        return Err("Decrypted block larger than expected".to_owned());
    }

    let pad = key.out_s - bytes.len();
    out_c[..pad].fill(0);
    out_c[pad..].copy_from_slice(&bytes);
    Ok(())
}

/// Decrypts RSA-encrypted data into a caller-provided output buffer.
///
/// The encrypted data is interpreted as a sequence of fixed-size RSA blocks
/// and decrypted using raw modular exponentiation.
///
/// On success, `*out_written` is set to the number of bytes written to `out`.
///
/// On failure:
/// - no guarantees are made about the contents of `out`,
/// - `*out_written` is not modified,
/// - `*err_msg` (if non-null) receives an allocated error string which
///   must be freed with `free_error`.
/// 
/// # Safety
///
/// - `key_pem` must be a valid pointer.
/// - `key_pem_len` must be the number of bytes readable at `key_pem`.
/// - If `data_len > 0`, `data` must point to a readable buffer of
///   exactly `data_len` bytes.
/// - If `out_cap > 0`, `out` must point to a writable buffer of
///   exactly `out_cap` bytes.
/// - `out_written` must be a valid, writable pointer to `usize`.
/// - `num_threads` must be greater than zero.
/// - `err_msg` must be either null or a valid, writable pointer to a
///   `*mut c_char`.
/// - The output buffer must be at least the size reported by
///   `get_decrypted_size`.
/// 
/// Passing invalid pointers, overlapping buffers, or incorrect lengths
/// results in undefined behavior.
#[no_mangle]
pub unsafe extern "C" fn decrypt(
    key_pem: *const u8,
    key_pem_len: usize,
    data: *const u8,
    data_len: usize,
    out: *mut u8,
    out_cap: usize,
    num_threads: usize,
    out_written: *mut usize,
    err_msg: *mut *mut c_char,
) -> c_int {
    clear_err(err_msg);

    if out_written.is_null() || num_threads == 0 {
        set_err(err_msg, "Invalid argument");
        return Bhderr::BHD_ERR_INVALID_ARG as c_int;
    }
    if key_pem_len > 0 && key_pem.is_null() {
        set_err(err_msg, "key_pem is null");
        return Bhderr::BHD_ERR_INVALID_ARG as c_int;
    }
    if data_len > 0 && data.is_null() {
        set_err(err_msg, "data is null");
        return Bhderr::BHD_ERR_INVALID_ARG as c_int;
    }
    if out_cap > 0 && out.is_null() {
        set_err(err_msg, "out is null");
        return Bhderr::BHD_ERR_INVALID_ARG as c_int;
    }

    let key_bytes = slice::from_raw_parts(key_pem, key_pem_len);
    let data = slice::from_raw_parts(data, data_len);
    let out = slice::from_raw_parts_mut(out, out_cap);

    let key = match parse_key_from_bytes(key_bytes) {
        Ok(k) => k,
        Err(e) => {
            set_err(err_msg, &e);
            return Bhderr::BHD_ERR_KEY_PARSE as c_int;
        }
    };

    let blocks = if data_len == 0 { 0 } else { data_len.div_ceil(key.in_s) };
    let required = blocks.saturating_mul(key.out_s);

    if required > out.len() {
        set_err(err_msg, "Output buffer too small");
        return Bhderr::BHD_ERR_OUTPUT_TOO_SMALL as c_int;
    }

    if required == 0 {
        *out_written = 0;
        return Bhderr::BHD_OK as c_int;
    }

    let out_used = &mut out[..required];

    let result = rayon::ThreadPoolBuilder::new()
        .num_threads(num_threads)
        .build()
        .map_err(|e| format!("Rayon pool error: {e}"))
        .and_then(|pool| {
            pool.install(|| {
                data.par_chunks(key.in_s)
                    .zip(out_used.par_chunks_mut(key.out_s))
                    .try_for_each(|(in_c, out_c)| decrypt_block(in_c, out_c, &key))
            })
        });

    match result {
        Ok(()) => {
            *out_written = required;
            Bhderr::BHD_OK as c_int
        }
        Err(e) => {
            set_err(err_msg, &e);
            Bhderr::BHD_ERR_INTERNAL as c_int
        }
    }
}
