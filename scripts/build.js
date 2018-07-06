#!/usr/bin/env node
'use strict';
var Path = require('path');

require('shelljs/global');
set('-e');

mkdir('-p', 'web_deploy')

cp('-R', 'web/*', 'web_deploy/');

exec('npm run build_open_api_generator');
exec('npm run run_open_api_generator');

cp('-R', 'output/*', 'web_deploy/');
rm('-rf', 'output')

var SWAGGER_UI_DIST = Path.dirname(require.resolve('swagger-ui'));
rm('-rf', 'web_deploy/swagger-ui/')
cp('-R', SWAGGER_UI_DIST, 'web_deploy/swagger-ui/')
sed('-i', 'http://petstore.swagger.io/v2/swagger.json', '../swagger.json', 'web_deploy/swagger-ui/index.html')

