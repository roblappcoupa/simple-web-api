
Write-Host "Building compose project and starting containers in interactive mode" -ForegroundColor Green

docker compose up --build -d

Write-Host "Containers are up and running" -ForegroundColor Green

