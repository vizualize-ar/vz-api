Created using commands found here: http://www.philipermish.com/blog/how-to-use-ssl-with-dotnet-core-and-kestrel/

openssl req -new -x509 -newkey rsa:2048 -keyout localhost.key -out localhost.cer -days 365 -subj /CN=localhost
used pass phrase "localhost" (without quotes)

then
openssl pkcs12 -export -out certificate.pfx -inkey localhost.key -in localhost.cer
used same phrase for passphrase and password