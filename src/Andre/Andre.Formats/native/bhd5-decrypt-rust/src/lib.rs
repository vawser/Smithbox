use core::slice;
use std::{ffi::{CStr, CString}, thread};
use num_bigint_dig::BigUint;
use pkcs1::{RsaPublicKeyDocument, DecodeRsaPublicKey, der::Document};

/// Reads a PEM-encoded PKCS1 key from a pointer to a null-terminated UTF-8 string
fn read_key(key: *const i8) -> Result<(BigUint, BigUint), String> {
    let c_str = unsafe {CStr::from_ptr(key)};
    let s = c_str.to_str().map_err(|e| format!("Error reading key string: {}", e))?;
    let doc = RsaPublicKeyDocument::from_pkcs1_pem(s).map_err(|e| format!("Error reading key: {}", e))?;
    let key = doc.decode();
    Ok((BigUint::from_bytes_be(key.modulus.as_bytes()), BigUint::from_bytes_be(key.public_exponent.as_bytes())))
}

/// Converts a given error message into a null-terminated UTF-8 string and places its address in err_ptr
fn write_err(err_ptr: *mut *const i8, err_message: &str) {
    let c_str = CString::new(err_message).unwrap();
    let raw = c_str.into_raw();
    unsafe {*err_ptr = raw};
}

/// Frees a returned error string. Must be called if an error string is returned to avoid a memory leak
#[no_mangle]
#[allow(clippy::missing_safety_doc)]
pub unsafe extern "C" fn free_error(err_ptr: *mut i8) {
    drop(CString::from_raw(err_ptr));
}

/// Calculates the maximum number of bytes that may be decrypted at once by an RSA modulus
fn input_block_size(modulus: &BigUint) -> usize {
    (modulus.bits() + 7) / 8
}

/// Calculates the number of bytes that wil result from decrypting the maximum number of input bytes for an RSA modulus
fn output_block_size(modulus: &BigUint) -> usize {
    (modulus.bits() - 1) / 8
}

/// Calculates the number of bytes that will result from decrypting a message with a given key
#[no_mangle]
pub extern "C" fn get_decrypted_size(data_len: i32, key: *const i8, err: *mut *const i8) -> i32 {
    let (modulus, _) = match read_key(key) {
        Ok(k) => k,
        Err(e) => {
            write_err(err, &e);
            return -1;
        }
    };
    let in_s = input_block_size(&modulus);
    let mut num_blocks = data_len as usize / in_s;
    let leftover = data_len as usize % in_s;
    if leftover != 0 {
        num_blocks += 1;
    }
    (output_block_size(&modulus) * num_blocks + 1) as i32
}

/// Helper function to be run by individual decryption threads
fn decrypt_inner(data: &[u8], output: &mut [u8], in_s: usize, out_s: usize, modulus: &BigUint, exp: &BigUint) -> Result<usize, String> {
    let mut out_ind = 0;
    for c in data.chunks(in_s) {
        let input = BigUint::from_bytes_be(c);
        let output_bi = input.modpow(exp, modulus);
        let bytes = output_bi.to_bytes_be();
        if output.len() < out_ind + out_s {
            return Err("Output array not big enough".to_string());
        }
        if bytes.len() < out_s {
            for _ in 0..(out_s - bytes.len()) {
                output[out_ind] = 0;
                out_ind += 1;
            }
        }
        output[out_ind..(out_ind + bytes.len())].copy_from_slice(&bytes);
        out_ind += bytes.len();
    }
    Ok(out_ind)
}

/// Decrypts RSA-encrypted data using a given key. 
/// Runs `num_threads` threads in parallel on equally-sized portions of the input data.
/// Speed will increase linearly with the number of threads up to a limit that is CPU-determined, but likely to be around the number of free cores
#[no_mangle]
#[allow(clippy::missing_safety_doc)]
pub unsafe extern "C" fn decrypt(data: *const u8, data_len: i32, output: *mut u8, output_size: i32, key: *const i8, num_threads: i32, err: *mut *const i8) -> i32 {
    let num_threads = num_threads as usize;
    if num_threads < 1 {
        write_err(err, "Number of threads cannot be less than 1");
        return -1;
    }
    let (modulus, exp) = match read_key(key) {
        Ok(k) => k,
        Err(e) => {
            write_err(err, &e);
            return -1;
        }
    };
    let data = slice::from_raw_parts(data, data_len as usize);
    let output = slice::from_raw_parts_mut(output, output_size as usize);
    let in_s = input_block_size(&modulus);
    let out_s = output_block_size(&modulus);
    let results: Vec<Result<usize, String>> = if num_threads == 1 {
        vec![decrypt_inner(data, output, in_s, out_s, &modulus, &exp)]
    } else {
        let num_blocks = (data_len as usize + in_s - 1) / in_s;
        let blocks_per_thread = (num_blocks + num_threads - 1) / num_threads;
        let mut handles = Vec::with_capacity(num_threads);
        let in_chunks = data.chunks(in_s * blocks_per_thread);
        let out_chunks = output.chunks_mut(out_s * blocks_per_thread);
        for (in_c, out_c) in in_chunks.zip(out_chunks) {
            let modulus = modulus.clone();
            let exp = exp.clone();
            handles.push(thread::spawn(move || {
                decrypt_inner(in_c, out_c, in_s, out_s, &modulus, &exp)
            }));
        }
        handles.into_iter().map(|t| t.join().unwrap()).collect()
    };
    let ans = results.into_iter().reduce(|a, b| {
        match a {
            Ok(s) => {
                if let Ok(s2) = b {
                    Ok(s + s2)
                } else {b}
            },
            Err(s) => {
                if let Err(s2) = b {
                    Err(format!("{}, {}", s, s2))
                } else {Err(s)}
            }
        }
    }).unwrap();
    match ans {
        Ok(s) => s as i32,
        Err(s) => {
            write_err(err, &s);
            -1
        }
    }
}