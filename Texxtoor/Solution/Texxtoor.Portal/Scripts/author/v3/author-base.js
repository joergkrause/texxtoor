var AUTHOR = (function (my) {
  // base functions that support easy ajax
  my.jsonPost = function(url, data) {
    return $.ajax({
      url: url,
      data: data,
      type: 'POST',
      contentType: "application/json; charset=utf-8",
      dataType: 'json'
    });
  };
  my.jsonGet = function(url, data) {
    return $.ajax({
      url: url,
      data: data,
      type: 'GET',
      contentType: "application/json; charset=utf-8",
      dataType: 'json'
    });
  };
  my.ajaxPost = function(url, data) {
    return $.ajax({
      url: url,
      data: data,
      type: 'POST',
      dataType: 'html'
    });
  };
  my.ajaxGet = function(url, data) {
    return $.ajax({
      url: url,
      data: data,
      type: 'GET',
      dataType: 'html'
    });
  };
  return my;
}(AUTHOR || {}));