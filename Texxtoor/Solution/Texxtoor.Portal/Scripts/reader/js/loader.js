function Loader() {
  var showForFlag = 0;
  var obj = $('.loader');
  this.show = function () { $('.view-container').append(obj); };
  this.hide = function () { obj.remove(); };
  this.showFor = function (jqueryObj) {
    jqueryObj.append(obj);
  };
  this.hideFor = function (jqueryObj) {
    jqueryObj.find('.loader').remove();
  };
}
