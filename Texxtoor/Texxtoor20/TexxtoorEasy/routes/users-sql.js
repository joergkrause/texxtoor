var async = require('async');
var express = require('express');
var router = express.Router();

module.exports = router;

/* GET users listing. */
router.get('/', function (req, res, next) {
  async.waterfall([
    function (callback) {
      getData(function (data) {
        callback(data);
      });
    }],
    function (result) {
    res.send({ data: result });
  });
});

// Connection object
var Connection = require('tedious').Connection;
// Request object
var Request = require('tedious').Request;
//
var config = {
  server: 'JOERGDEVELOPER',
  userName: 'Node_User',
  password: 'Node_User',
  options: {
    database: 'Node_EventBooking',
    instanceName: 'SQLEXPRESS'
  },
  encrypt: false // true for Azure users
};

var connection, request;

function getData(callback) {
  connection = new Connection(config);
  connection.on('connect', function () {
    executeStatement(callback);
  });
  connection.on('debug', function (txt) {
    console.log(txt);
  });
}

function executeStatement(callback) {
  request = new Request("select * from users", function (err, rowCount) {
    if (err) {
      console.log(err);
    } else {
      console.log(rowCount + ' rows');
      callback(res);
    }
    connection.close();
  });
  var res = [];
  
  request.on('row', function (columns) {
    var d = {};
    columns.forEach(function (column) {
      if (column.value === null) {
        console.log('NULL');
      } else {
        d[column.metadata.colName] = column.value;
        console.log(column.value);
      }
    });
    res.push(d);
  });
  
  request.on('done', function (rowCount, more) {
    console.log(rowCount + ' rows returned');    
  });
  
  // In SQL Server 2000 you may need: connection.execSqlBatch(request);
  connection.execSql(request);
}