#Start-Process -FilePath "emulators/cosmosdb/cosmosdb.emulator.exe"
Start-Process -FilePath "emulators/eventgrid/eventgridemulator.exe" -WorkingDirectory "emulators/eventgrid"
#cmd.exe /k 'azurite --silent --location c:\azurite --debug c:\azurite\debug.log\'