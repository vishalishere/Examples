var http = require('http');

function make_api_call(op, args, success, failure) {
  console.log('make_api_call', op, args);
  var options = {
    hostname: 'api.globalhack4.test.lockerdome.com',
    path: '/' + op,
    method: 'POST',
  };

  var data = new Buffer(0);

  var req = http.request(options, function(res) {
    res.on('data', function (chunk) {
      data = Buffer.concat([data, chunk]);
    });
    res.on('end', function(){
      try {
        var api_response = JSON.parse(data.toString('utf8'));
      } catch (ex) {
        return failure("JSON parse error: [" + ex +
                       "] while parsing: [" + data.toString('utf8') + "]", ex.stack);
      }
      if(api_response.status) return success(api_response);
      else return failure(api_response.error_message);
    });
  });
  req.on('error', function(e) {
    return failure("Error on request to api: " + e.message);
  });
  req.write(JSON.stringify(args));
  req.end();
};

var APP_ID = 7739353396674626;
var APP_SECRET = "rO+g4J8qPMbgid3e21EGDDNZAvYgcSjqaQqxhCetEfIyStX8Uk4Im2pTlt1tAYolyHJhUAazN487F8TG1a7j1g==";

function app_fetch_content(content_id, success, failure){
  make_api_call('app_fetch_content', {
    app_id: APP_ID,
    app_secret: APP_SECRET,
    content_id: content_id
  }, success, failure);
}

function app_create_content(name, thumb_url, text, app_data, created_by, login_token, success, failure){
  make_api_call('app_create_content', {
    app_id: APP_ID,
    app_secret: APP_SECRET,
    name: name,
    thumb_url: thumb_url,
    text: text,
    app_data: app_data,
    created_by: created_by,
    login_token: login_token
  }, success, failure);
}

var server = http.createServer(function(req, res){
  var query_string_start_position = req.url.indexOf('?') + 1;

  var url_base = query_string_start_position ? req.url.slice(0, query_string_start_position - 1) : req.url;
  var frame_data = query_string_start_position ? JSON.parse(decodeURIComponent(req.url.slice(query_string_start_position))) : {};
  console.log(url_base, frame_data);

  var content_id = frame_data.args ? frame_data.args.id : null;

  function respond(body){
    res.writeHead(200, {
      'Content-Length': body.length,
      'Content-Type': 'text/html'
    });
    res.write(body);
    res.end();
  }

  app_fetch_content(content_id, function(response_data){
    var body = [
      "<html>",
      "  <head>",
      "    <script src='http://globalhack4.test.lockerdome.com/gh_app_platform.js'></script>",
      "  </head>",
      "  <body>",
      "    <table>",
      "    <tr><td>Frame Data:</td><td>" + JSON.stringify(frame_data) + "</td></tr>",
      "    <tr><td>Response Data:</td><td>" + JSON.stringify(response_data) + "</td></tr>",
      "    <tr><td colspan=2><button onclick='LD.request_height(500);'>Make Taller</button></td></tr>",
      "    </table>",
      "  </body>",
      "</html>"
    ].join("\n");

    respond(body);
  }, function(error){
    respond(error);
  });
});

server.listen(80);
