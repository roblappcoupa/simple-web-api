#!/bin/sh

# Substitute environment variables in nginx.conf and output to a temp file
envsubst '${LISTEN_PORT} ${PROXY_HOST}' < /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf

# Start NGINX
nginx -g 'daemon off;'
