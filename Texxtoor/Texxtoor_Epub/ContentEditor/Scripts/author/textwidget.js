﻿(function ($) {
    $.fn.htmlarea = function (opts) {
        if (opts && typeof (opts) === "string") {
            var args = [];
            for (var i = 1; i < arguments.length; i++) { args.push(arguments[i]); }
            var htmlarea = txxtrEditWidget(this);
            if (htmlarea == null) return;
            var f = htmlarea[opts];
            if (f) { return f.apply(htmlarea, args); }
        }
        return this.each(function () { txxtrEditWidget(this, opts); }); // surounding DIV
    };

    var txxtrEditWidget = window.txxtrEditWidget = function (elem, options) {
        if (elem != null) {
            if (elem.jquery) {
                return txxtrEditWidget(elem[0]);
            }
            if (elem.txxtrEditWidgetObject) {
                return elem.txxtrEditWidgetObject;
            } else {
                return new txxtrEditWidget.fn.init(elem, options);
            }
        }
    };
    txxtrEditWidget.fn = txxtrEditWidget.prototype = {
        txxtrEditWidget: "1.0.0",
        opts: {},
        snippetId: 0,
        isActivated: false,
        //Resize table column
        minColumnWidth: 50,
        //Select table range
        selectionStart: null,
        selectionEnd: null,
        inSelect: false,
        inSelectThead: false,
        selectionParent: null,
        leadColBgColor: '#4F81BD',
        table: null,
        editor: null,
        container: null,
        currentCharPos: 0,
        range: null,
        changeTimer: null,
        init: function (elem, options) {
            var $this = this;
            $this.editor = $(elem).find('div.editor');
            if ($this.editor) {
                // merge options
                var opts = $this.opts = $.extend({}, txxtrEditWidget.defaultOptions, options);
                // remember myself
                elem.txxtrEditWidgetObject = $this;
                $this.snippetId = $(elem).data('item');
                $this.editor.attr('data-item', $this.snippetId);

                priv.initEditor.call($this, opts);
                priv.attachEditorEvents.call($this);

                if ($(elem).is(".editableTable")) {
                    $this.table = $this.editor.find(' > table');
                    $this.table.addClass('editableTableInner');
                    $this.activateTableStyles();
                    $this.updateTableStyles();
                }
                if (opts.loaded) { opts.loaded.call($this); }

            }
            return this;
        },
        dispose: function () {
            this.textarea.show().insertAfter(this.container);
            this.container.remove();
            this.textarea[0].txxtrEditWidgetObject = null;
        },
        execCommand: function (a, b, c) {
            var $this = this;
            this.editor.focus();
            document.execCommand(a, b || false, c || null);
            $this.opts.onSave($this.editor.parent());
        },
        ec: function (a, b, c) {
            this.execCommand(a, b, c);
        },
        queryCommandValue: function (a) {
            this.iframe[0].contentWindow.focus();
            return this.editor.queryCommandValue(a);
        },
        qc: function (a) {
            return this.queryCommandValue(a);
        },
        getSelectedHTML: function () {
            if ($.browser.msie) {
                return this.getRange().htmlText;
            } else {
                var elem = this.getRange().cloneContents();
                return $("<div/>").append($(elem)).html();
            }
        },
        getSelection: function () {
            if ($.browser.msie) {
                return window.document.selection;
            } else {
                return window.getSelection();
            }
        },
        getRange: function () {
            var s = this.getSelection();
            if (!s) { return null; }
            return (s.getRangeAt) ? s.getRangeAt(0) : s.createRange();
        },
        html: function (v) {
            if (v) {
                this.pasteHTML(v);
            } else {
                return toHtmlString();
            }
        },
        pasteEquation: function (equation) {
            if (this.equation) {
                this.equation.after($(equation));
                this.equation = null;
                author.jscroll.setContentPositionAfterInsert();
            } else if (this.range) {
                this.pasteHTML(equation, this.range);
                this.range = null;
                author.jscroll.setContentPositionAfterInsert();
            } else {
                alert('Select a text where you want the equation to paste in.');
            }
            
        },
        pasteHTML: function (html, r) {
            var range;
            if (!r) range = this.getRange();
            else range = this.range;
            if ($.browser.msie) {
                range.pasteHTML(html);
            } else if ($.browser.mozilla) {
                range.deleteContents();
                range.insertNode($((html.indexOf("<") != 0) ? $("<span/>").append(html) : html)[0]);
            } else { // Safari
                range.deleteContents();
                range.insertNode($("<span/>").append($((html.indexOf("<") != 0) ? "<span>" + html + "</span>" : html))[0]);
            }
            range.collapse(true);
            this.editor.focus();
            this.opts.onSave(this.editor.parent());
        },
        insertIndex: function () {
            var index = "<span class='isindex' data-type='index' title='" + this.getSelectedHTML() + "'>" + this.getSelectedHTML() + "</span>";
            this.pasteHTML(index);
        },
        cut: function () {
            if ($.browser.msie) {
                this.ec("cut");
            } else {
                author.clipboard = this.getSelectedHTML();
                this.getRange().deleteContents();
            }
        },
        copy: function () {
            if ($.browser.msie) {
                this.ec("copy");
            } else {
                author.clipboard = this.getSelectedHTML();
            }
        },
        paste: function () {
            if ($.browser.msie) {
                this.ec("paste");
            } else {
                this.pasteHTML(author.clipboard);
            }
        },
        cleanUp: function () {
            body = this.editor;
            _activeRTEData = $(body).html();
            beginLen = $.trim($(body).html()).length;

            setTimeout(function () {
                var text = $(body).html();
                var newLen = $.trim(text).length;
                caret = 0;
                for (i = 0; i < newLen; i++) {
                    if (_activeRTEData[i] != text[i]) {
                        caret = i - 1;
                        break;
                    }
                }
                if (caret == -1) caret = 0;
                var origText = text.slice(0, caret);
                var newText = text.slice(caret, newLen - beginLen + caret + 4);
                var tailText = text.slice(newLen - beginLen + caret + 4, newLen);
                var newText = newText.replace(/(.*(?:endif-->))|([ ]?<[^>]*>[ ]?)|(&nbsp;)|([^}]*})/g, '');
                newText = newText.replace(/[·]/g, '');
                $(body).html(origText + newText + tailText);
                $(body).contents().last().focus();
            }, 100);
        },
        bold: function () { this.ec("bold"); },
        italic: function () { this.ec("italic"); },
        underline: function () { this.ec("underline"); },
        strikeThrough: function () { this.ec("strikethrough"); },
        image: function (url) {
            if ($.browser.msie && !url) {
                this.ec("insertImage", true);
            } else {
                this.ec("insertImage", false, (url || prompt("Image URL:", "http://")));
            }
        },
        removeFormat: function () {
            this.ec("removeFormat", false, []);
            this.unlink();
        },
        link: function () {
            if ($.browser.msie) {
                this.ec("createLink", true);
            } else {
                this.ec("createLink", false, prompt("Link URL:", "http://"));
            }
        },
        unlink: function () { this.ec("unlink", false, []); },
        undo: function () { this.ec("undo") },
        selectall: function () { this.ec("selectall") },
        orderedList: function () { this.ec("insertorderedlist"); },
        unorderedList: function () { this.ec("insertunorderedlist"); },
        superscript: function () { this.ec("superscript"); },
        subscript: function () { this.ec("subscript"); },
        p: function () {
            this.formatBlock("<p>");
        },
        indent: function () {
            this.ec("indent");
        },
        outdent: function () {
            this.ec("outdent");
        },
        insertHorizontalRule: function () {
            this.ec("insertHorizontalRule", false, "ht");
        },
        justifyLeft: function () {
            this.ec("justifyLeft");
        },
        justifyCenter: function () {
            this.ec("justifyCenter");
        },
        justifyRight: function () {
            this.ec("justifyRight");
        },
        increaseFontSize: function () {
            if ($.browser.msie) {
                this.ec("fontSize", false, this.qc("fontSize") + 1);
            } else if ($.browser.safari) {
                this.getRange().surroundContents($(this.iframe[0].contentWindow.document.createElement("span")).css("font-size", "larger")[0]);
            } else {
                this.ec("increaseFontSize", false, "big");
            }
        },
        decreaseFontSize: function () {
            if ($.browser.msie) {
                this.ec("fontSize", false, this.qc("fontSize") - 1);
            } else if ($.browser.safari) {
                this.getRange().surroundContents($(this.iframe[0].contentWindow.document.createElement("span")).css("font-size", "smaller")[0]);
            } else {
                this.ec("decreaseFontSize", false, "small");
            }
        },
        forecolor: function (c) {
            this.ec("foreColor", false, c || prompt("Enter HTML Color:", "#"));
        },
        formatBlock: function (v) {
            this.ec("formatblock", false, v || null);
        },
        toHtmlString: function () {
            return this.editor.html();
        },
        toString: function () {
            return this.editor.text();
        },
        /* Table Widget Support */
        /************** Prepare Functions ***************/
        // widget must call this to apply styles on a "per table" base
        activateTableStyles: function () {
            var $this = this;
            if ($this.isActivated) return;
            $this.table.find('th, td').each(function () {
                $(this).removeAttr('width');
                $(this).width($(this).width());
                $(this).attr('selected', false);
                $(this).removeClass('selected');
            });
            $this.tableResizable();
            $this.table.find('td, th').live('mousedown', function (e) {
                //                if (!$('.tablePane').is(":visible")) {
                //                    $('.pane').hide();
                //                    $('.tablePane').show().find('a').click();
                //                }
                if (!$(this).is(".selected")) {
                    $this.table.find('td, th').attr('selected', false);
                    $this.table.find('td, th').removeClass('selected');
                    $(this).attr('selected', true);
                    $(this).addClass('selected');
                } else {
                    $this.table.find('td, th').attr('selected', false);
                    $this.table.find('td, th').removeClass('selected');
                    $this.inSelect = false;
                    $this.inSelectThead = false;
                }
                $this.selectionStart = $(this);
                if ($(this).closest("thead").length > 0) {
                    $this.inSelectThead = true;
                } else if ($(this).closest("tbody").length > 0) {
                    $this.inSelect = true;
                }

            });

            $this.table.find('td, th').live('mouseup', function () {
                if ($this.table.find('.selected').length > 0) {
                    $('.ins,.del').parent().removeClass('disabled').fadeTo("100", "1");
                    $('.ins,.del').parent().removeAttr('disabled').fadeTo("100", "1");
                } else {
                    $('.ins,.del').parent().addClass('disabled').fadeTo("100", "0.4");
                    $('.ins,.del').parent().attr('disabled', 'disabled').fadeTo("100", "0.4");
                }
                $this.inSelect = false;
                $this.inSelectThead = false;
                $this.selectionStart = null;
                $this.selectionEnd = null;
                $this.table.removeClass('no-user-select');
                $this.table.enableTextSelect();
            });
            $this.table.find('thead td, thead th').live('mousemove', function (e) {
                if (!$this.inSelectThead)
                    return;
                if ($this.selectionEnd && $(this)[0] == $this.selectionEnd[0])
                    return;
                $this.table.addClass('no-user-select');
                $this.selectionEnd = $(this);
                $this.selectTableRange();
            });
            $this.table.find('tbody td, tbody th').live('mousemove', function () {
                if (!$this.inSelect)
                    return;
                if ($this.selectionEnd && $(this)[0] == $this.selectionEnd[0])
                    return;
                $this.table.addClass('no-user-select');
                $this.table.disableTextSelect();
                $this.selectionEnd = $(this);
                $this.selectTableRange();
            });
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find('.table-type .ribbon-button').click(function () {
                $('.table-type .ribbon-button').removeClass("active");
                $(this).addClass("active");
            });
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find('.setTableOptions').click(function () {
                $this.setTableOptions();
            });
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find('.showTableOptions').click(function () {
                $this.showTableOptions();
            });
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find(".col-width input").live('change', function () {
                var val = $(this).val();
                var i = $(this).parent().index();
                val = parseInt(val < $this.minColumnWidth ? $this.minColumnWidth : val);

                var c1 = $this.table.c[i];
                var c2 = $this.table.c[i + 1] ? $this.table.c[i + 1] : $this.table.c[i - 1];
                if (c1.w == val) return;
                var inc = val - c1.w;
                var w1 = c1.w + inc;
                var w2 = c2.w - inc;
                if (w2 < $this.minColumnWidth) {
                    w2 = $this.minColumnWidth;
                    w1 = c1.w + c2.w - w2;
                }
                c1.width(w1);
                c2.width(w2);
                c1.w = w1; c2.w = w2;
                $this.updateTableData();
            });
            $this.updateTableStyles();
            this.isActivated = true;
        },
        tableResizable: function () {
            var $this = this;
            var t = $this.table;
            var drag = null;                                // reference to the current grip that is being dragged

            var init = function () {
                t.prev().remove();                          // remove grips container  
                t.g = [];                                   // grips array
                t.c = [];                                   // columns array
                t.w = t.width();                            // table width
                t.gc = $('<div class="grips"/>');           // grips container
                t.before(t.gc);                             // the grips container object is added
                t.b = parseInt($.browser.msie ? t.get(0).border || t.get(0).currentStyle.borderLeftWidth : t.css('border-left-width')) || 1;    // outer border width (again cross-browser isues)
                t.cs = parseInt($.browser.msie ? t.get(0).cellSpacing || t.get(0).currentStyle.borderSpacing : t.css('border-spacing')) || 2;   // table cellspacing (not even jQuery is fully cross-browser)

                var mc = 0, cc = 0, ri = 0;
                t.find('tr').each(function (i) {
                    cc = $(this).children('td, th').length;
                    if (cc > mc) {
                        ri = i;
                        mc = cc;
                    }
                });
                t.mc = mc;                                  // max columns
                t.ri = ri;                                  // index of row with max columns
                createGrips();                              // grips are created
            };
            var createGrips = function () {
                var tr = t.find('tr:eq(' + t.ri + ')');
                var cl = tr.children('th, td');
                cl.each(function (i) {
                    var g = $('<div class="grip"></div>');
                    var c = $(this);                                        // jquery wrap for the current column
                    c.w = c.width(); g.c = c; g.i = i;                      // some values are stored in the grip's node data
                    c.width(c.w).removeAttr("width");                       // the width of the column is converted into pixel-based measurements
                    t.gc.append(g);                                         // add the visual node to be used as grip
                    t.g.push(g); t.c.push(c);                               // the current grip and column are added to its table object
                    if (i < cl.length - 1) g.mousedown(onGripMouseDown);    // bind the mousedown event to start dragging 
                    else g.addClass("last-grip").removeClass("grip");       // the last grip is used only to store data
                });
                syncGrips();                            // the grips are positioned according to the current table layout
                t.find('th, td').each(function () {
                    $(this).removeAttr('width');        // the width attribute is removed from all table cells which are not nested in other tables and dont belong to the header
                });
            };
            var syncGrips = function () {
                t.gc.width(t.w);                        // the grip's container width is updated
                for (var i = 0; i < t.mc; i++) {        // for each column
                    var c = t.c[i];
                    t.g[i].css({                        // height and position of the grip is updated according to the table layout
                        left: c.offset().left - t.offset().left + c.outerWidth() + t.cs / 2 + 'px',
                        height: t.outerHeight()
                    });
                }
            };
            var syncCols = function (i, isOver) {
                var inc = drag.x - drag.l, c = t.c[i], c2 = t.c[i + 1];
                var w = c.w + inc; var w2 = c2.w - inc;             // their new width is obtained					
                c.width(w + 'px'); c2.width(w2 + 'px');             // and set	
                if (isOver) { c.w = w; c2.w = w2; }
            };
            var onGripMouseDown = function (e) {
                var g = $(this);
                $this.activateEditor();
                g = t.g[g.index()];
                t.parent().bind('mousemove.tableResizable', onGripDrag).bind('mouseup.tableResizable', onGripDragOver); // mousemove and mouseup events are bound
                g.ox = e.pageX; g.l = g.position().left; // the initial position is kept
                drag = g;
                drag.addClass('dragged');
                return false; 	// prevent text selection
            };
            var onGripDrag = function (e) {
                if (!drag) return;
                var x = e.pageX - drag.ox + drag.l; 	// next position according to horizontal mouse position increment
                var i = drag.i;
                var l = t.cs * 1.5 + $this.minColumnWidth + t.b;
                var max = i == t.mc - 1 ? t.w - l : t.g[i + 1].position().left - t.cs - $this.minColumnWidth;     // max position according to the contiguous cells
                var min = i ? t.g[i - 1].position().left + t.cs + $this.minColumnWidth : l; 			            // min position according to the contiguous cells
                x = Math.max(min, Math.min(max, x)); 					            // apply boundings		
                drag.x = x; drag.css("left", x + 'px'); 			        // apply position increment
                syncCols(i); syncGrips();
                author.jscroll.setContentPositionAfterResize();
                return false; 	// prevent text selection
            };
            var onGripDragOver = function (e) {
                t.parent().unbind('mousemove.tableResizable').unbind('mouseup.tableResizable');
                if (!drag) return;
                if (drag.x) { 									//only if the column width has been changed
                    syncCols(drag.i, true); syncGrips();              // the columns and grips are updated
                }
                drag.removeClass('dragged');
                drag = null;            // since the grip's dragging is over
                return false; 	// prevent text selection
            };
            init();
        },
        /************** ROW Functions ***************/
        duplicatelastrow: function () {
            $(this.editor.body).find('table.editableTableInner tbody > tr:last').clone(true).insertAfter('table tbody > tr:last');
            $this.updateTableStyles();
            $this.updateTableData();
        },
        appendrow: function () {
            var $this = this;
            var row = "<tr></tr>";
            var col = $this.maxCols($this.table);
            if ($this.table.find('tbody > tr:last').length > 0) {
                row = $this.table.find('tbody > tr:last').after(row);
                row = row.next();
            } else {
                $this.table.find('tbody').append(row);
                row = $this.table.find('tbody tr:first');
            }

            for (var i = 0; i < col; i++) {
                $(row).append("<td>&nbsp;</td>");
            }
            $this.tableResizable();
            $this.updateTableStyles();
            $this.updateTableData();
            author.jscroll.setContentPositionAfterResize();
        },
        insertrow: function () {
            var $this = this;
            var row = "<tr></tr>";
            var col = $this.maxCols($this.table);
            var idx1 = $this.table.find('.selected:last').parent().children(":first").index();
            var idx2 = $this.table.find('.selected:last').parent().children(":last").index();
            var index = 0;
            var i = 0;
            $this.table.find('tr').each(function () {
                if ($(this).find('.selected').length > 0) {
                    index = i;
                }
                i++;
            });
            $this.selectrange(idx1, idx2, index, index);
            i = 0;
            $this.table.find('tr').each(function () {
                if ($(this).find('.selected').length > 0) {
                    index = i;
                }
                i++;
            });
            row = $this.table.find("tr:eq(" + index + ")").after(row);;
            for (var i = 0; i < col; i++) {
                $(row.next()).append("<td>&nbsp;</td>");
            }
            $this.tableResizable();
            $this.updateTableStyles();
            $this.updateTableData();
            author.jscroll.setContentPositionAfterResize();
        },
        deleterow: function () {
            var $this = this;
            var idx2 = 0;
            var index = 0;
            var i = 0;
            var rowIndex = 0;
            var idx1 = $this.table.find('th.selected:last, td.selected:last').index();
            $this.table.find('tr').each(function () {
                if ($(this).find('.selected:last').length > 0) {
                    idx2 = $(this).find('.selected:last').parent().children(":last").index();
                    index = i;
                }
                i++;
            });
            var find = false;
            for (var j = index; j > 0; j--) {
                $this.table.find('tr:eq(' + j + ') td, tr:eq(' + j + ') th').each(function () {
                    $(this).addClass('selected');
                    if (this.rowSpan > 1) return;
                    rowIndex = j;
                    find = true;
                });
                if (find) break;
            }
            $this.selectrange(0, idx2, rowIndex, index);
            $this.splitCells();
            $this.selectrange(0, idx2, rowIndex, index);
            $this.table.find('th.selected:last, td.selected:last').parent(":last").remove();
            $this.tableResizable();
            $this.updateTableStyles();
            $this.updateTableData();

        },
        /************** COL Functions ***************/
        insertfirstcolumn: function () {
            $(this.table).find('tbody > tr > td:first-child').before('<td>&nbsp;</td>');
            $(this.table).find('thead > tr > th:first-child').before('<th>&nbsp;</th>');
            $this.tableResizable();
            $this.updateTableStyles();
            $this.updateTableData();
        },
        insertcolumn: function () {
            var index = $(this.table).find('th.selected:last, td.selected:last').index();
            this.insertcolumnafter(index);
        },
        insertcolumnafter: function (idx) {
            var $this = this;
            $this.selectrange(idx, idx, 0, $this.table.find('tr').length - 1);
            $this.splitCells();
            $this.selectrange(idx, idx, 0, $this.table.find('tr').length - 1);

            $this.table.find('thead > tr').each(function () {
                var tr = $(this);
                $(this).find('td, th').each(function () {
                    if ($(this).width() > ($this.minColumnWidth + ($this.minColumnWidth / (tr.find('td, th').length - 1)))) {
                        $(this).width($(this).width() - ($this.minColumnWidth / (tr.find('td, th').length - 1)) - tr.find('td, th').length * 4);
                    }
                });
                $(this).find('td.selected:last, th.selected:last').after('<th style="width: ' + $this.minColumnWidth + 'px;">&nbsp;</th>');
            });
            $this.table.find('tbody > tr').each(function () {
                $(this).find('td.selected:last, th.selected:last').after('<td style="width: ' + $this.minColumnWidth + 'px;">&nbsp;</td>');
            });

            $this.tableResizable();
            $this.updateTableStyles();
            $this.updateTableData();
        },
        appendcolumn: function () {
            var index = $(this.table).find('thead th:last, thead td:last').index();
            this.insertcolumnafter(index);
        },
        deletecolumn: function () {
            var $this = this;
            var idx = $this.table.find('th.selected:last, td.selected:last').index();
            $this.selectrange(idx, idx, 0, $this.table.find('tr').length - 1);
            $this.splitCells();
            $this.selectrange(idx, idx, 0, $this.table.find('tr').length - 1);
            $this.table.find('tbody > tr').each(function () { $(this).find('td.selected:last, th.selected:last').remove(); });
            $this.table.find('thead > tr').each(function () { $(this).find('td.selected:last, th.selected:last').remove(); });
            $this.tableResizable();
            $this.updateTableStyles();
            $this.updateTableData();
        },
        // Update Styles
        updateTableStyles: function () {
            var $this = this;
            $this.table.find("tr:odd").each(function () {
                $(this).css("background", "#D3DFEE");
            });
            $this.table.find("tr:even").each(function () {
                $(this).css("background", "#FFFFFF");
            });
            $this.table.find('th, td').css("background", 'transparent');
            var color = $this.table.is(".leadcol") ? 'white' : 'black';
            var bgColor = $this.table.is(".leadcol") ? $this.leadColBgColor : 'transparent';
            var rows = $this.table.find("tr").length;
            $this.table.find('thead th, thead td').css("background", bgColor);
            $this.table.find('thead tr th, thead tr td').css("color", color);
            var i = 0;
            while (i < rows) {
                var rowSpan = $this.table.find("tr:eq(" + i + ") td:first, tr:eq(" + i + ") th:first").is("[rowspan]") ? $this.table.find("tr:eq(" + i + ") td:first").attr("rowspan") : 1;
                $this.table.find("tr:eq(" + i + ") td:first, tr:eq(" + i + ") th:first").css("background", bgColor);
                $this.table.find("tr:eq(" + i + ") td:first, tr:eq(" + i + ") th:first").css("color", color);
                i += parseInt(rowSpan);
            }
            $('.ins,.del').parent().addClass('disabled').fadeTo("100", "0.4");
            $('.ins,.del').parent().attr('disabled', 'disabled').fadeTo("100", "0.4");
            $this.table.find('td, th').attr('selected', false);
            $this.table.find('td, th').removeClass('selected');
        },
        updateTableData: function () {
            var $this = this;
            if ($this.table.length == 0) return;
            var cols = $this.maxCols($this.table);
            var rows = $this.table.find("tr").length;
            var rowHeight = $this.table.find("tr:first").height();
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find('.table-type .ribbon-button[data-option="' + $this.table.attr('class').split(' ')[1] + '"]').addClass("active");
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='rows']").val(rows);
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='row-height']").val(rowHeight);
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='cellspacing']").val($this.table.attr('cellspacing'));
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='cols']").val(cols);
            var columns = $this.table.find('tr:first').children('th, td').length;
            var colWidthInput = '<input type="text" value="" />';
            var colWidthBody = $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find('.col-width > div');
            colWidthBody.html('');
            var item;
            var maxCols = 0;
            var currCols = 0;
            var rowIndex = 0;
            $this.table.find('tr').each(function (index) {
                currCols = $(this).children('td, th').length;
                if (currCols > maxCols) {
                    rowIndex = index;
                    maxCols = currCols;
                }
            });
            $this.table.find('tr:eq(' + rowIndex + ')').children('th, td').each(function (index) {
                item = $('<div></div>').append("<div>" + (index + 1) + "</div>").append(colWidthInput);
                item.find('input').val($(this).width())
                colWidthBody.append(item);
                item.find('input').spinner({
                    min: 50,
                    max: 1000,
                    step: 1,
                    increment: 'fast'
                });
            });
        },
        showTableOptions: function () {
            var $this = this;
            $(".tableOptionsDialog:not([data-item='" + $this.snippetId + "'])").hide();
            if ($(".tableOptionsDialog[data-item='" + $this.snippetId + "']").is(":visible")) {
                $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").hide();
            } else {

                $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").show();
                $this.tableResizable(); $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[type='text']").spinner({
                    min: 0,
                    max: 1000,
                    step: 1,
                    increment: 'fast'
                });
                $this.updateTableData();
            }
        },
        setTableOptions: function () {
            var $this = this;
            if ($this.table.length == 0) return;

            var maxCols = $this.maxCols($this.table);
            var maxRows = $this.table.find('tr').length;
            var rows = ($(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='rows']").val() >= 1) ? $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='rows']").val() : 1;
            var cols = ($(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='cols']").val() >= 1) ? $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='cols']").val() : 1;
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='rows']").val(rows);
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='cols']").val(cols);
            var tableType = $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find(".ribbon-button.active").data('option');
            var cellList = $this.cellList($this.table);
            var rowHeight = ($(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='row-height']").val() >= 20) ? $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='row-height']").val() : 20;
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='row-height']").val(rowHeight);
            $this.table.find("tr").each(function () {
                $(this).height(rowHeight);
            });
            var cellspacing = ($(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='cellspacing']").val() >= 0) ? $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='cellspacing']").val() : 0;

            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").find("input[name='cellspacing']").val(cellspacing);

            $this.table.attr('cellspacing', cellspacing);
            for (var i = 0; i < $this.table.c.length; i++) {
                var c = $this.table.c[i];
                c.width(c.w - (cellspacing * 1));
                c.w = c.width();
            }
            $this.table.removeClass('simple leadcol grid');
            $this.table.addClass(tableType);
            if (cols > maxCols) {
                var appendCols = cols - maxCols;
                for (var i = 0; i < appendCols; i++) {
                    $this.appendcolumn();
                }
            } else {
                var deleteCols = maxCols - cols;
                var maxIdx = 0;
                for (var i = 0; i < deleteCols; i++) {
                    for (var j = 0; j < maxCols - i; j++) {
                        if ($(cellList[0 + '-' + j]).length > 0) {
                            maxIdx = j;
                        }
                    }
                    $this.updateTableStyles();
                    $(cellList[0 + '-' + maxIdx]).addClass('selected');
                    $this.deletecolumn();
                }
            }
            if (rows > maxRows) {
                var appendRows = rows - maxRows;
                for (var i = 0; i < appendRows; i++) {
                    $this.appendrow();
                }
            } else {
                var deleteRows = maxRows - rows;
                for (var i = 0; i < deleteRows; i++) {
                    $this.updateTableStyles();
                    $this.table.find('tr:last').children('th, td').addClass('selected');
                    $this.deleterow();
                }
            }
            $this.tableResizable();
            $this.updateTableStyles();
            $(".tableOptionsDialog[data-item='" + $this.snippetId + "']").hide();
            author.jscroll.setContentPositionAfterResize();
        },
        // Merge and split cells
        mergecells: function () {
            var $this = this;
            if ($this.table.find('.selected').length == 1) return;
            var minRow = Number.MAX_VALUE, minColumn = Number.MAX_VALUE, maxRow = 0, maxColumn = 0;
            var matrix = [];
            var cols = 0, rows = 0;
            $this.table.find("tr").each(function () {
                cols = 0;
                $(this).children('td, th').each(function () {
                    var colSpan = $(this).is("[colspan]") ? $(this).attr("colspan") - 1 : 0;
                    var rowSpan = $(this).is("[rowspan]") ? $(this).attr("rowspan") - 1 : 0;

                    for (var j = rows; j <= rows + rowSpan; j++) {
                        for (var i = cols; i <= cols + colSpan; i++) {
                            if (!matrix[j])
                                matrix[j] = [];
                            while (matrix[j][i]) {
                                cols++;
                                i++;
                            }
                            matrix[j][i] = $(this);

                            if ($(this).is('.selected')) {
                                minRow = Math.min(minRow, rows);
                                minColumn = Math.min(minColumn, cols);
                                maxColumn = Math.max(maxColumn, cols);
                                maxRow = Math.max(maxRow, rows);
                            }
                        }
                    }
                });
                rows++;
            });
            var $cell = matrix[minRow][minColumn];
            $cell.attr('rowspan', maxRow - minRow + 1).attr('colspan', maxColumn - minColumn + 1);
            for (var j = 0; j < matrix.length; j++)
                for (var i = 0; i < matrix[j].length; i++)
                    if (matrix[j][i].is('.selected') && matrix[j][i][0] != $cell[0])
                        matrix[j][i].remove();

            $this.table.find('td, th').attr('selected', false);
            $this.table.find('td, th').removeClass('selected');
            $this.tableResizable();
            $this.updateTableData();
            $this.updateTableStyles();
        },
        selectTableRange: function (top, left, bottom, right, matrix) {
            var $this = this;
            if (!$this.selectionStart || !$this.selectionEnd)
                return;
            if (!matrix) {
                var column1, row1, column2, row2;
                var matrix = [];
                var cols = 0, rows = 0;
                $this.table.find('tr').each(function () {
                    cols = 0;
                    $(this).children('td, th').each(function () {
                        var colSpan = $(this).is("[colspan]") ? $(this).attr("colspan") - 1 : 0;
                        var rowSpan = $(this).is("[rowspan]") ? $(this).attr("rowspan") - 1 : 0;
                        for (var j = rows; j <= rows + rowSpan; j++) {
                            for (var i = cols; i <= cols + colSpan; i++) {
                                if (!matrix[j])
                                    matrix[j] = [];
                                while (matrix[j][i]) {
                                    cols++;
                                    i++;
                                }
                                matrix[j][i] = $(this);

                                if ($(this)[0] == $this.selectionStart[0]) {
                                    column1 = cols;
                                    row1 = rows;
                                }
                                if ($(this)[0] == $this.selectionEnd[0]) {
                                    column2 = cols;
                                    row2 = rows;
                                }
                            }
                        }
                    });
                    rows++;
                });

                bottom = Math.max(row1, row2);
                top = Math.min(row1, row2);
                left = Math.min(column1, column2);
                right = Math.max(column1, column2);

                $this.table.find('td, th').removeClass('selected');
                $this.table.find('td, th').attr('selected', false);

                $this.selectTableRange(top, left, bottom, right, matrix);
            } else {
                var rangeWasNotChanged = 0;
                while (rangeWasNotChanged < 2) {
                    $(matrix).each(function (row) {
                        $(this).each(function (column) {
                            if ($(this).is('.selected') || ((left <= column && column <= right) && (top <= row && row <= bottom))) {
                                if (!$(this).is('.selected')) {
                                    rangeWasNotChanged = 0;
                                    $(this).addClass('selected');
                                    $(this).attr('selected', true);
                                }

                                var new_bottom = Math.max(bottom, row);
                                var new_top = Math.min(top, row);
                                var new_left = Math.min(left, column);
                                var new_right = Math.max(right, column);

                                if (new_bottom != bottom
		   		            			|| new_top != top
		   		            			|| new_left != left
		   		            			|| new_right != right) {
                                    rangeWasNotChanged = 0;
                                    bottom = new_bottom;
                                    top = new_top;
                                    left = new_left;
                                    right = new_right;
                                }
                            }
                        });
                    });
                    rangeWasNotChanged += 1;
                }
            }
        },
        selectrange: function (idx1, idx2, start, end) {
            var $this = this;
            var cellList = $this.cellList($this.table);
            $this.inSelect = true;
            $this.inSelectThead = true;
            $this.selectionStart = cellList[start + '-' + idx1];
            $this.selectionEnd = cellList[end + '-' + idx2];
            $this.selectTableRange();
            $this.inSelect = false;
            $this.inSelectThead = false;
        },
        splitcells: function () {
            var $this = this;
            $this.splitCells();
            $this.tableResizable();
            $this.updateTableStyles();
            $this.updateTableData();
        },
        splitCells: function () {
            var $this = this;
            $('.selected', $this.table).each(function () {
                var colSpan = $(this).is("[colspan]") ? $(this).attr("colspan") - 1 : 0;
                var rowSpan = $(this).is("[rowspan]") ? $(this).attr("rowspan") - 1 : 0;
                if (!colSpan && !rowSpan)
                    return;

                var $cell = $(this);
                var selectedCol = Number.MAX_VALUE, selectedRow = Number.MAX_VALUE;
                var matrix = [];
                var cols = 0, rows = 0;

                $this.table.find("tr").each(function () {
                    cols = 0;
                    $(this).children('td, th').each(function () {
                        var colSpan = $(this).is("[colspan]") ? $(this).attr("colspan") - 1 : 0;
                        var rowSpan = $(this).is("[rowspan]") ? $(this).attr("rowspan") - 1 : 0;

                        for (var j = rows; j <= rows + rowSpan; j++) {
                            for (var i = cols; i <= cols + colSpan; i++) {
                                if (!matrix[j])
                                    matrix[j] = [];
                                while (matrix[j][i]) {
                                    cols++;
                                    i++;
                                }
                                matrix[j][i] = $(this);

                                if ($(this)[0] == $cell[0]) {
                                    selectedCol = Math.min(selectedCol, cols);
                                    selectedRow = Math.min(selectedRow, rows);
                                }
                            }
                        }
                    });
                    rows++;
                });

                $(this).removeAttr("colspan");
                $(this).removeAttr("rowspan");

                var firstRow = true;
                var rowSpanCopy = rowSpan;
                while (rowSpanCopy > -1) {
                    if (!firstRow) {
                        $cell = null
                        if (selectedCol > 0) {
                            var k = 1;
                            while (matrix[selectedRow + rowSpan - rowSpanCopy - 1] && matrix[selectedRow + rowSpan - rowSpanCopy][selectedCol - k][0] == matrix[selectedRow + rowSpan - rowSpanCopy - 1][selectedCol - k][0])
                                k++;
                            if (selectedCol - k >= 0)
                                $cell = $('<td>&nbsp</td>').insertAfter(matrix[selectedRow + rowSpan - rowSpanCopy][selectedCol - k]);
                        }
                        if ($cell == null) {
                            $cell = $('<td>&nbsp</td>').prependTo($('tr', $this.table).get(selectedRow + rowSpan - rowSpanCopy));
                        }
                    }
                    var colSpanCopy = colSpan;
                    while (colSpanCopy > 0) {
                        $cell = $('<td>&nbsp</td>').insertAfter($cell);
                        colSpanCopy--;
                    }
                    rowSpanCopy--;
                    firstRow = false;
                }
            });
        },
        maxCols: function (table) {
            var tr = $(table).get(0).rows,  // define number of rows in current table
			span, 			                // sum of colSpan values
			max = 0, 		                // maximum number of columns
			i, j; 			                // loop variable

            for (i = 0; i < tr.length; i++) {
                span = 0;
                for (j = 0; j < tr[i].cells.length; j++) {
                    span += tr[i].cells[j].colSpan || 1;
                }
                if (span > max) {
                    max = span;
                }
            }
            return max;
        },
        cellList: function (table) {
            var lookup = {}, cell, i = 0, j = 0;
            $(table).find('tr').each(function () {
                $(this).children('th, td').each(function () {
                    lookup[i + '-' + j] = $(this);
                    j++;
                });
                i++;
                j = 0;
            });

            return lookup;
        },
        /* Events */
        activateEditor: function () {
            this.opts.onActivate(this.editor.parent(), this.snippetId);
        },
        leaveEditor: function () {
            if (typeof (this.opts.onLeave) === "function") {
                this.opts.onLeave(this.snippetId);
            }
        }
    };
    txxtrEditWidget.fn.init.prototype = txxtrEditWidget.fn;
    txxtrEditWidget.defaultOptions = {
        css: null,
        onActivate: {},
        onLeave: {},
        onSave: {}
    };
    var priv = {
        initEditor: function (options) {
            var $this = this;
            $this.editor.live('focus', function () {
                var elm = $(this);
                elm.data('before', elm.html());
            }).live('keyup paste', function () {
                var elm = $(this);
                clearTimeout($this.changeTimer);
                author.jscroll.setContentPositionAfterResize();
                $this.changeTimer = setTimeout(function () {
                    if (elm.data('before') !== elm.html()) {
                        elm.data('before', elm.html());
                        elm.trigger('change');
                        $this.editor.find('p, span').filter(function () {
                            return $(this).html().length == 0;
                        }).remove();
                    }
                }, 2500);
            });
            $this.editor.live('change', function () {
                options.onSave($this.editor.parent());
            });
        },
        attachEditorEvents: function () {
            var $this = this;
            var fn = function (e) {
                $this.activateEditor();
            };
            var fnmu = function (e) {
                if ($(e.target).parents('.equationContainer').length > 0) {
                    $this.equation = $(e.target).parents('.equationContainer');
                } else {
                    $this.range = $this.getRange();
                }
            };
            $this.editor.click(fn).
               keyup(fn).
               keydown(fn).
               mousedown(fn).
               focusin(fn).
               mouseup(fnmu);
            $this.editor.mouseleave($this.leaveEditor());
        },
        isArray: function (v) {
            return v && typeof v === 'object' && typeof v.length === 'number' && typeof v.splice === 'function' && !(v.propertyIsEnumerable('length'));
        }
    };
})(jQuery);