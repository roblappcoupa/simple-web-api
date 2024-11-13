FROM nginx:latest

# Copy the nginx.conf template
COPY nginx.conf /etc/nginx/nginx.conf.template

# Copy the entrypoint script
COPY entrypoint.sh /entrypoint.sh

# Make the entrypoint script executable
RUN chmod +x /entrypoint.sh

# Set the entrypoint
ENTRYPOINT ["/entrypoint.sh"]
