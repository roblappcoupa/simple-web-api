docker build -t nginx-proxy .
docker run -d -p 5011:5011 nginx-proxy
