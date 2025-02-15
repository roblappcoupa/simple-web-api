#!/bin/sh

# Substitute environment variables in nginx.conf and output to a template file
envsubst '${LISTEN_PORT} ${PROXY_HOST} ${BUFFERING} ${TIMEOUT} ${SHORT_TIMEOUT}' < /etc/nginx/nginx.conf.template > /etc/nginx/nginx.conf

# Start NGINX
nginx -g 'daemon off;'
