FROM nginx:latest

# Copy the nginx.conf as an nginx template
COPY nginx.conf /etc/nginx/nginx.conf.template

# Copy the entrypoint script
COPY entrypoint.sh /entrypoint.sh

# Ensure Unux line endings and make it executable
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

# Install fswatch
RUN apt-get update && \
    apt-get install -y bash && \
    apt-get install -y tree && \
    apt-get install -y procps && \
    apt-get install -y iproute2 && \
    apt-get install -y net-tools && \
    apt-get install -y man-db && \
    rm -rf /var/lib/apt/lists/*

# Set the entrypoint
ENTRYPOINT ["/entrypoint.sh"]
