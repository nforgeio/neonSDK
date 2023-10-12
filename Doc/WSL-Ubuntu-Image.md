# Maintainer: Create Ubuntu 20.04 WSL image

The **neon-ubuntu-20.04.** WSL distro starts off as a standard Ubuntu download from the MSFT
store.  The only change so far is that we add the **/etc/wsl.conf** file:

```
[interop]
appendWindowsPath=False
```

Then export the distro to a TAR file, compress it and then upload it to S3, like:

```
wsl --export neon-ubuntu-20.04 "$env:TEMP\neon-ubuntu-20.04.tar"
pigz --best --blocksize 512 "$env:TEMP\neon-ubuntu-20.04.tar"
ren "%TEMP%\neon-ubuntu-20.04.tar.gz" "$env:TEMP\neon-ubuntu-20.04.tar"

Finally upload the compressed TAR file to S3:

1. Upload compressed TAR file to S3  to: https://neon-public.s3.us-west-2.amazonaws.com/build-assets/wsl/neon-ubuntu-20.04.tar
2. Edit S3 metadata: Content-Encoding=gzip
```
