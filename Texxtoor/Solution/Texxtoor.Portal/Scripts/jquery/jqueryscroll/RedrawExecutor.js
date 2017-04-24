/* 
Helper object which support queued execution of one method. 
So if execution is now in progress we put method in queue, but if queue contains smth we replace that. So only last method is executed. 
Only public method is execute;

Parameters:
options = {
startTimeout: <integer value which shows if we need to wait a timeout before delegate execution>
}
*/
function RedrawExecutor(options) {
    this._executing = false;
    this._working = false;
    $.extend(this._options, options);
}
RedrawExecutor.prototype = {
    _queuedMethod: null,
    _executing: null,
    _working: null,
    _options: {
        startTimeout: 10
    },

    _start: function () {
        if (!this._queuedMethod) {
            this._executing = false;
            return;
        }
        if (this._executing) {
            return;
        }

        this._executing = true; // Show that execution is started
        var $this = this;

        function executeDelegate() {
            var method = $this._queuedMethod;
            $this._queuedMethod = null;
            method();
            this._executing = false;
            $this._start();
        }

        if (this._options.startTimeout) {
            setTimeout(executeDelegate, this._options.startTimeout);
        } else {
            executeDelegate();
        }
    },

    execute: function (delegate) {
        this._queuedMethod = delegate;
        this._start();
    }
}