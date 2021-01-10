@echo off
set curl_get=curl -X GET -s -w "\n%%{http_code}\n"
set curl_post=curl -X POST -s -w "\n%%{http_code}\n"
set curl_put=curl -X PUT -s -w "\n%%{http_code}\n"
set curl_del=curl -X DELETE -s -w "\n%%{http_code}\n"

echo Should return empty array (200)
%curl_get% http://localhost:2200/messages

echo Create first message per POST and return its ID (201)
%curl_post% http://localhost:2200/messages -d "Neue Nachricht (per POST)"

echo Create second message per PUT (201)
%curl_post% http://localhost:2200/messages -d "Nachricht 2 (per PUT)"

echo Create third message per POST (201)
%curl_post% http://localhost:2200/messages -d "Nachricht 3 (per POST)"

echo Should return first message (200)
%curl_get% http://localhost:2200/messages/1

echo Should list the three messages (200)
%curl_get% http://localhost:2200/messages

echo Should update the first message (200)
%curl_put% http://localhost:2200/messages/1 -d "Text wurde aktualisiert (per PUT)"

echo Should remove the third message (200)
%curl_del% http://localhost:2200/messages/3

echo Should list the two remaining messages (one created per POST, one updated per PUT) (200)
%curl_get% http://localhost:2200/messages

echo Should fail to remove the third message (404)
%curl_del% http://localhost:2200/messages/3

pause
