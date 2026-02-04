use std::{fs::File, io::{Read, Write}, ffi::{CStr, CString}, time::Instant};

mod lib;

pub fn main() {
    let start = Instant::now();
    let key = r#"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEA0iDVVQ230RgrkIHJNDgxE7I/2AaH6Li1Eu9mtpfrrfhfoK2e7y4O
WU+lj7AGI4GIgkWpPw8JHaV970Cr6+sTG4Tr5eMQPxrCIH7BJAPCloypxcs2BNfT
GXzm6veUfrGzLIDp7wy24lIA8r9ZwUvpKlN28kxBDGeCbGCkYeSVNuF+R9rN4OAM
RYh0r1Q950xc2qSNloNsjpDoSKoYN0T7u5rnMn/4mtclnWPVRWU940zr1rymv4Jc
3umNf6cT1XqrS1gSaK1JWZfsSeD6Dwk3uvquvfY6YlGRygIlVEMAvKrDRMHylsLt
qqhYkZNXMdy0NXopf1rEHKy9poaHEmJldwIFAP////8=
-----END RSA PUBLIC KEY-----"#;
    let filename = r#"C:\SteamLibrary\steamapps\common\Elden Ring\Game\data2.bhd"#;
    let mut f = File::open(filename).unwrap();
    let mut ans = vec!();
    let bytes = f.read_to_end(&mut ans).unwrap();
    println!("{}", bytes);
    let err: *const i8 = std::ptr::null::<i8>();
    let err_ptr = (&err) as *const *const i8 as *mut *const i8;
    let key_cstr = CString::new(key).unwrap();
    let key_bytes = key_cstr.as_ptr() as *const i8;
    let dec_size = lib::get_decrypted_size(bytes as i32, key_bytes, err_ptr);
    println!("{}", dec_size);
    if !err.is_null() {
        println!("{:?}", unsafe {CStr::from_ptr(err)});
        return;
    }
    let mut dec = Vec::with_capacity(dec_size as usize);
    let len;
    unsafe {
        len = lib::decrypt(ans.as_ptr(), bytes as i32, dec.as_mut_ptr(), dec.capacity() as i32, key_bytes, 8, err_ptr);
    }
    if !err.is_null() {
        println!("{:?}", unsafe {CStr::from_ptr(*err_ptr)});
        return;
    }
    unsafe {dec.set_len(len as usize);}
    let end = start.elapsed();
    println!("{}", end.as_secs_f32());
    f = File::create("output.bhd.decrypted").unwrap();
    f.write_all(&dec).unwrap();
}