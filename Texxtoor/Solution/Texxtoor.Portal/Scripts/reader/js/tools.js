Array.prototype.single = function(delegate) {
    if (delegate && typeof(delegate) == 'function') {
        for (var i = 0; i < this.length; ++i) {
            if (delegate(this[i])) return this[i];
        }
    }
    return null;
};
Array.prototype.each = function(delegate) {
    if (delegate && typeof(delegate) == 'function') {
        var arr = [];
        for (var i = 0; i < this.length; ++i) {
            arr.push(delegate(this[i]));
        }
        return arr;
    }
    return this;
};
Array.prototype.pushAll = function(arr) {
    if (arr) {
        var $this = this;
        arr.each(function(item) {
            $this.push(item);
        });
    }
    return this;
};
Array.prototype.where = function(delegate) {
    if (delegate && typeof(delegate) == 'function') {
        var arr = [];
        for (var i = 0; i < this.length; ++i) {
            if (delegate(this[i])) arr.push(this[i]);
        }
        return arr;
    }
    return this;
};
Array.prototype.last = function() {
    return this[this.length - 1];
};
Array.prototype.first = function() {
    return this[0];
};
Array.prototype.orderBy = function(strField, boolDescending) {
    var a = null, b = null, c = null;
    if (strField) {
        var fields = strField.split('.');
        a = fields[0] ? fields[0] : null;
        b = fields[1] ? fields[1] : null;
        c = fields[2] ? fields[2] : null;
    }
    if (boolDescending)
        this.sort(function(x, y) {
            var arg1 = x, arg2 = y;
            if (a && x[a]) {
                arg1 = x[a];
                arg2 = y[a];
                if (b && x[a][b]) {
                    arg1 = x[a][b];
                    arg2 = y[a][b];
                    if (c && x[a][b][c]) {
                        arg1 = x[a][b][c];
                        arg2 = y[a][b][c];
                    }
                }
            }
            if (arg1 < arg2) return 1;
            else if (arg1 > arg2) return -1;
            return 0;
        });
    else
        this.sort(function(x, y) {
            var arg1 = x, arg2 = y;
            if (a && x[a]) {
                arg1 = x[a];
                arg2 = y[a];
                if (b && x[a][b]) {
                    arg1 = x[a][b];
                    arg2 = y[a][b];
                    if (c && x[a][b][c]) {
                        arg1 = x[a][b][c];
                        arg2 = y[a][b][c];
                    }
                }
            }
            if (arg1 < arg2) return -1;
            else if (arg1 > arg2) return 1;
            return 0;
        });
    return this;
};
Array.prototype.isEmpty = function() {
    if (this.length > 0) return false;
    return true;
};