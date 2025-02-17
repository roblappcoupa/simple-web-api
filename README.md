# Simple API and Proxy

## Health Checks

### upstream web api
```
curl http://localhost:5013/health -v -i
```

### web api
```
curl http://localhost:5012/health -v -i
```

### api gateway
```
curl http://localhost:5011/health -v -i
```

### top level proxy
```
curl http://localhost:5010/health -v -i
```

## Http Connection Info

### upstream web api
```
curl http://localhost:5013/connection -v -i
```

### web api
```
curl http://localhost:5012/connection -v -i
```

### api gateway
```
curl http://localhost:5011/connection -v -i
```

### top level proxy
```
curl http://localhost:5010/connection -v -i
```


## Timeouts and Delays

### upstream web api
Client side timeout (connection + request time)
```
curl http://localhost:5013/api/v1/test/delay?delay=10 -v -i --max-time 5
```

### upstream web api
Client side timeout (connection)
```
curl http://localhost:5013/api/v1/test/delay?delay=10 -v -i --connect-timeout 5
```

### top level proxy
Proxy timeout
```
curl http://localhost:5010/buffering-off/api/v1/test/delay?delay=10 -v -i --connect-timeout 5
```

### top level proxy
Proxy timeout
```
curl http://localhost:5010/timeout/api/v1/test/delay?delay=10 -v -i
```

