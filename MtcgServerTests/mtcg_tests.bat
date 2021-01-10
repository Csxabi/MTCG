@echo off
:setup
set curl_get=curl -X GET -s -w "\n%%{http_code}\n" -H "Authorization: %auth%"
set curl_post=curl -X POST -s -w "\n%%{http_code}\n" -H "Authorization: %auth%"
set curl_put=curl -X PUT -s -w "\n%%{http_code}\n" -H "Authorization: %auth%"
set curl_del=curl -X DELETE -s -w "\n%%{http_code}\n" -H "Authorization: %auth%"
if "%cont%" NEQ "" (goto %cont%)

echo Login with wrong credentials, should fail
%curl_post% http://localhost:10001/login -d "{\"username\":\"gibtsned\", \"password\":\"nope\"}"

echo Login with correct credentials, should work
%curl_post% -D - http://localhost:10001/login -d "{\"username\": \"tester1\",\"password\": \"tester1pw\"}"

set /p auth=Copy Authorization value here: 
set cont=postauth
goto setup

:postauth
echo Get own profile
%curl_get% http://localhost:10001/profile

echo Get profile of tester1 (own, for checking)
%curl_get% http://localhost:10001/profile/tester1

echo Get profile of tester2
%curl_get% http://localhost:10001/profile/tester2

echo Get own stack
%curl_get% http://localhost:10001/stack

echo Get own deck
%curl_get% http://localhost:10001/deck

pause
