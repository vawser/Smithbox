This is a simple implementation of RSA public key decryption written in Rust intended to help decrypt .bhd files. The latest built .dll should always be included elsewhere, so you shouldn't need to touch this unless you want to mess with the RSA decryption specifically, or you want to build for a platform other than Windows.

To build the library, [install Rust](https://rust-lang.org/tools/install/) if you haven't already, and run this command:

```
cargo build --lib --release
```