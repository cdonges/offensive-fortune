#!/bin/bash

# Query requests from the last 24 hours with readable formatting

sqlite3 data/requests.db ".mode column
.headers on
SELECT DateTime(Timestamp, 'localtime') as Time, IpAddress, Method, Path FROM RequestLogs WHERE Timestamp > datetime('now', '-1 day') ORDER BY Timestamp DESC;
SELECT COUNT(*) as Total_Requests_24h FROM RequestLogs WHERE Timestamp > datetime('now', '-1 day');"
