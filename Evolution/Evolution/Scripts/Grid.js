// Construction and Initialisation
var gridManager = new GridManager();

function GridManager() {
    this.grids = new Array();
}

GridManager.prototype.InitialiseGrid = function (gridOptions) {
    var idx = this.grids.length;
    gridOptions.index = idx;
    gridOptions.totalitems = 0;
    if (!gridOptions.width) gridOptions.width = '100%';
    if (!gridOptions.pagesizes) gridOptions.pagesizes = [30, 50, 100];
    if (!gridOptions.hasOwnProperty('clearmessageondisplay')) gridOptions.clearmessageondisplay = true;
    this.grids.push(gridOptions);

    LoadGridState(idx);

    this.RenderGrid(idx);
    DisplayGrid(idx);
};

// Render an entire grid
GridManager.prototype.RenderGrid = function (idx) {
    var gridData = this.grids[idx];
    var html = '<div id="' + gridData.container + 'head"></div>';
    html += '<div id="' + gridData.container + 'list" style="width:100%;overflow-x:auto;border-right:1px solid #DDDDDD"></div>';
    html += '<div id="' + gridData.container + 'foot"></div>';
    $('#' + gridData.container).html(html);

    this.RenderHeader(idx);
    this.RenderList(idx);
    this.RenderFooter(idx);
}

// Render the header of a grid
GridManager.prototype.RenderHeader = function (idx) {
    var gridData = this.grids[idx];
    var gotDates = false;
    var gotTimes = false;
    var html = '';
    if (gridData.addurl || gridData.showsearch || gridData.searchfields) {
        html = '<div class="panel"><div class="panel-body"><div class="row">';
        var maxWidth = 12;
        var leftHtml = ''; var leftWidth = 0;
        var rightHtml = '';

        if (gridData.addurl) leftWidth += 2;
        if (gridData.restoreurl) leftWidth += 2;
        if (gridData.searchfieldsleft) leftWidth = gridData.searchfieldsleft;
        var rightWidth = maxWidth - leftWidth;

        if (leftWidth > 0) {
            leftHtml = '<div class="col-sm-' + leftWidth + '" style="padding:0;">';
            if (gridData.addurl) leftHtml += this.CreateAddHtml(idx);
            if (gridData.restoreurl) leftHtml += this.CreateRestoreHtml(idx);
        }
        if (rightWidth > 0) rightHtml = '<div class="col-sm-' + rightWidth + ' text-right" style="padding:0;">';

        if (gridData.showsearch || gridData.searchfields) {
            if (gridData.searchfields) {
                for (var i = 0; i < gridData.searchfields.length; i++) {
                    var searchField = gridData.searchfields[i];
                    var fldWidth = '100px';
                    if (searchField.width) fldWidth = searchField.width;
                    maxWidth -= fldWidth;
                    var fldName = gridData.container + '_' + searchField.id;
                    var fldDefValue = searchField.defaultvalue;
                    if (!fldDefValue) fldDefValue = '';
                    var fldValue = GetCookie(fldName, fldDefValue);
                    var fldType = searchField.type;

                    var fieldHtml = '';
                    if (fldType == 'dropdownlist') {
                        fieldHtml += '<select id="' + fldName + '" class="form-control"';
                        fieldHtml += ' style="display:inline;margin-right:4px;';
                        if (searchField.width) fieldHtml += 'width:' + searchField.width;
                        fieldHtml += '"';
                        if (searchField.refreshonchange) fieldHtml += ' onchange="GridSearch(' + idx + ', \'txtSearch\')"';
                        fieldHtml += '>';
                        var temp;
                        if (searchField.datasourceurl) {
                            temp = DoAjaxCall(searchField.datasourceurl);
                            for (var j = 0; j < temp.length; j++) {
                                fieldHtml += this.CreateOption(temp[j].Id, temp[j].Text, fldValue);
                            }
                        } else if (searchField.datasource) {
                            temp = searchField.datasource.split('|');
                            for (var j = 0; j < temp.length; j++) {
                                var keyValue = temp[j].split('=');
                                if (keyValue.length == 1) {
                                    fieldHtml += this.CreateOption(keyValue[0], keyValue[0], fldValue);
                                } else {
                                    fieldHtml += this.CreateOption(keyValue[0], keyValue[1], fldValue);
                                }
                            }
                        }
                        fieldHtml += '</select>';

                    } else if (fldType == 'date') {
                        fldDefValue = '';
                        if (!gridData.displaydateformat) {
                            alert('date header field requires displaydateformat to be specified in header gridOption settings!');
                        } else {
                            var momFormat = gridData.displaydateformat.toUpperCase();     // dd/MM/yyyy
                            fldDefValue = moment().format(momFormat);
                        }
                        fldValue = GetCookie(fldName, fldDefValue);
                        fieldHtml += '<input type="text" id="' + fldName + '" class="form-control" style="display:inline;margin-right:4px;width:' + fldWidth + '" maxlength="10" value="' + fldValue + '"/>';
                        gotDates = true;

                    } else if (fldType == 'datetime') {
                        fldDefValue = '';
                        if (!gridData.displaydateformat) {
                            alert('datetime header field requires displaydateformat to be specified in header gridOption settings!');
                        } else {
                            var momFormat = gridData.displaydateformat.toUpperCase();     // dd/MM/yyyy
                            fldDefValue = moment().format(momFormat) + ' ' + fldDefValue;
                        }
                        fldValue = GetCookie(fldName, fldDefValue);
                        fieldHtml += '<input type="text" id="' + fldName + '" class="form-control" style="display:inline;margin-right:4px;width:' + fldWidth + '" maxlength="19" value="' + fldValue + '"/>';
                        gotDates = true;
                        gotTimes = true;

                    } else if (fldType == 'text') {
                        fieldHtml += '<input type="text" id="' + fldName + '" class="form-control" style="display:inline;margin-right:4px;width:' + fldWidth + '" maxlength="' + searchField.maxlength + '" value="' + fldValue + '"';
                        if (searchField.placeholder) fieldHtml += ' placeholder="' + searchField.placeholder + '"';
                        fieldHtml += '/>';

                    } else if (fldType == 'label') {
                        fieldHtml += '<span style="width:' + fldWidth + ';margin-right:8px">';
                        fieldHtml += searchField.text;
                        fieldHtml += '</span>';

                    } else if (fldType == 'button') {
                        // Grid Header Buttons
                        if (searchField.url) {
                            fieldHtml += '<a href="';
                            fieldHtml += this.DoSubstitutions(searchField.url, 0, 0, 0, 0, idx);
                            fieldHtml += '" onclick="DisplayProgress();SaveGridState(' + idx + ');"';
                            fieldHtml += ' class="button ';
                            if (searchField.class) {
                                fieldHtml += searchField.class;
                            } else {
                                fieldHtml += 'button';
                            }
                            if (!searchField.width) fieldHtml += ' button';
                            fieldHtml += '"data-toggle="tooltip" data-placement="right" title="' + searchField.text + '" style="margin-top:5px;"><span class="glyphicon glyphicon-edit"></span><span class="button-text">' + searchField.text + '</span></a>&nbsp;&nbsp;';
                        } else {
                            fieldHtml += '<button type="button" class="';
                            if (searchField.class) {
                                fieldHtml += searchField.class;
                            } else {
                                fieldHtml += 'button add';
                            }

                            // text, rowNum, colNum, fieldNum, rowId, idx
                            temp = searchField.jsfunc;
                            fieldHtml += '" onclick="return ' + temp + '" data-toggle="tooltip" data-placement="right" title="' + searchField.text + '" style="margin-top:5px;">';
                            if (searchField.icon)
                                fieldHtml += '<span class="glyphicon glyphicon-' + searchField.icon + '"></span></span></button>';
                            else
                                fieldHtml += '<span class="glyphicon glyphicon-plus"></span></span></button>';
                        }
                    }
                    if (searchField.align == 'left') {
                        leftHtml += fieldHtml;
                    } else {
                        rightHtml += fieldHtml;
                    }
                }
            }
            if (leftHtml != '') leftHtml += '</div>';

            if (gridData.showsearch) {
                // Default search field and button
                rightHtml += '<input type="text" id="txtSearch' + idx + '" value="' + GetCookie(gridData.container + ':search', '') + '" class="form-control" style="margin-right:4px;width:160px;display:inline;" maxlength="100"/>';
                rightHtml += '<button type="button" id="btnSubmit' + idx + '" class="button" onclick="return GridSearch(' + idx + ', \'txtSearch\')" data-toggle="tooltip" data-placement="left" title="Search"><span class="glyphicon glyphicon-search"></span></button>';
            }
            html += (leftHtml + rightHtml);
            html += '</div>';
        } else {
            html += leftHtml + '</div>';
            if (rightHtml != '') html += rightHtml + '</div>';
        }

        html += '</div></div></div>';
    }
    $('#' + gridData.container + 'head').html(html);
    if (gotDates || gotTimes) {
        // Set date and time formats to header search fields after the fields have been created
        //$.datepicker.setDefaults({ dateFormat: gridData.jqdateformat, altFormat: '' });

        var options = {
            dateFormat: gridData.jqdateformat,
            altFormat: ''
        };
        if (gotTimes) {
            options.controlType = 'select';
            options.oneLine = true;
            options.timeFormat = 'HH:mm:ss';
        }

        for (var i = 0; i < gridData.searchfields.length; i++) {
            if (fldType == 'date') {
                $('#' + gridData.container + '_' + gridData.searchfields[i].id).datepicker(options);
            } else if(fldType == 'datetime') {
                $('#' + gridData.container + '_' + gridData.searchfields[i].id).datetimepicker(options);
            }
        }
    }
    // Apply the enter key to the search field
    if (gridData.showsearch) {
        $('#txtSearch' + idx).keypress(function (event) {
            if (event.which == 13) $('#btnSubmit' + idx).click();
        });
    }
}

GridManager.prototype.CreateOption = function (value, text, selected) {
    var rc = '<option value="' + value;
    rc += '"';
    if (value == selected) rc += ' selected';
    rc += '>' + text + '</option>';
    return rc;
}

GridManager.prototype.CreateAddHtml = function (idx) {
    var gridData = this.grids[idx];
    var addHtml = '';
    var addHtml = '';
    if (gridData.addurl) {
        addHtml = '<a href="';
        addHtml += this.DoSubstitutions(gridData.addurl, 0, 0, 0, 0, idx);
        addHtml += '" onclick="DisplayProgress();SaveGridState(' + idx + ');" class="button add"';
        if (gridData.addtext) {
            addHtml += ' data-toggle="tooltip" data-placement="right" title="' + gridData.addtext + '" style="margin-top:5px;">';
        } else {
            addHtml += ' data-toggle="tooltip" data-placement="right" title="Add" style="margin-top:5px;">';
        }
        if (gridData.icon) {
            addHtml += '<span class="glyphicon glyphicon-' + gridData.icon + '"></span>';
        } else {
            addHtml += '<span class="glyphicon glyphicon-plus"></span>';
        }
        addHtml += '</a>';
    }
    return addHtml;
}

GridManager.prototype.CreateRestoreHtml = function (idx) {
    var gridData = this.grids[idx];
    var html = '';
    if (gridData.restoreurl) {
        html = '<button type="button" class="btn btn-sm btn-default btn-width-xl" style="margin-right:4px;height:34px;padding-top:3px"';
        html += ' href="#"';
        html += ' onclick="return GridConfirmRestoreDefaults(' + idx + ')">';
        if (gridData.restoretext) {
            html += gridData.restoretext;
        } else {
            html += 'Restore Defaults';
        }
        html += '</button > ';
    }
    return html;
}

// Render the list area of a grid
GridManager.prototype.RenderList = function (idx) {
    var gridData = this.grids[idx];
    var temp;
    var fldName;

    var html = '<table class="table-condensed table-striped table-bordered table-hover';
    if (gridData.tableclass) html += ' ' + gridData.tableclass;
    html += '"';
    if (gridData.width) html += ' style="width:' + gridData.width + '"';
    html += '>';
    html += '<thead><tr>';
    for (var k = 0; k < gridData.columnDefs.length; k++) {
        if (!gridData.columnDefs[k].hidden) {
            html += '<th';
            if (gridData.columnDefs[k].class) html += ' class="' + gridData.columnDefs[k].class + '"';
            if (gridData.columnDefs[k].width) {
                var twidth = 0;
                if (gridData.sortcolumns && gridData.sortcolumns.indexOf(fldName) != -1) twidth = 20;
                html += ' style="width:' + this.ParseWidth(gridData.columnDefs[k].width, twidth) + '"';
            }
            html += '>';

            fldName = gridData.columnDefs[k].fields[0].field;
            if (gridData.sortcolumns && gridData.sortcolumns.indexOf(fldName) != -1) {
                html += '<div class="small" style="width:auto;display:inline;">';
                html += gridData.columnDefs[k].heading;
                html += '</div><div style="width:20px;display:inline;float:right;">';

                html += '&nbsp;<a href="javascript:GridSort(' + idx + ',\'' + fldName + '\',';
                if (fldName == gridData.sortcolumn) {
                    // Got the sort column
                    if (gridData.sortorder == 1) {
                        html += '0)"><img src="/Content/ArrowUpBlue.png';
                    } else {
                        html += '1)"><img src="/Content/ArrowDownBlue.png';
                    }
                } else {
                    // Not the current sort column
                    html += '0)"><img src="/Content/ArrowUp.png';
                }
                html += '" style="width:6px;height:6px"/></a></div>';
            } else {
                html += '<span class="small">' + gridData.columnDefs[k].heading + "</span>";
            }
            html += '</th>';
        }
    }
    html += '</tr></thead>';

    html += '<tbody>';
    if (gridData.data) {
        for (var i = 0; i < gridData.data.length; i++) {
            html += '<tr>';
            for (var j = 0; j < gridData.columnDefs.length; j++) {
                if (!gridData.columnDefs[j].hidden) {
                    html += '<td class="text-top';
                    if (gridData.columnDefs[j].class) {
                        html += ' ' + gridData.columnDefs[j].class;
                    } else {
                        html += ' small';
                    }
                    html += '">';
                    for (var l = 0; l < gridData.columnDefs[j].fields.length; l++) {
                        var columnDef = gridData.columnDefs[j].fields[l];
                        if (!columnDef.hidden) {
                            fldName = this.CreateFieldName(columnDef.id, i, j, l);
                            var fieldValue = this.FormatField(gridData.data[i][columnDef.field], columnDef);

                            switch (columnDef.type) {
                                case 'text':
                                    fieldValue = columnDef.text;
                                    html += this.CreateTextHtml(columnDef, fieldValue, idx, i);
                                    break;

                                case 'field':
                                case 'html':
                                    html += this.CreateFieldHtml(columnDef, fieldValue, gridData.data[i], idx, i, fldName);
                                    break;

                                case 'link':
                                    html += this.CreateLinkHtml(columnDef, fieldValue, idx, i);
                                    break;

                                case 'isodate':
                                case 'isotime':
                                case 'isodatetime':
                                case 'isodatetimebr':
                                    fieldValue = gridData.data[i][gridData.columnDefs[j].fields[l].field];
                                    html += this.CreateISODateHtml(columnDef, fieldValue, idx, i);
                                    break;

                                case 'email':
                                    html += this.CreateEMailHtml(columnDef, fieldValue, idx, i);
                                    break;

                                case 'checkbox':
                                    fieldValue = gridData.data[i][gridData.keyfield];
                                    html += this.CreateCheckBoxHtml(columnDef, fieldValue, idx, i);
                                    break;

                                //case 'radiobutton':
                                //    html += this.CreateRadioButtonHtml(columnDef, fieldValue, idx, i);
                                //    break;

                                case 'hidden':
                                    html += this.CreateHiddenHtml(columnDef, fieldValue, idx, i);
                                    break;

                                case 'button':
                                case 'buttonedit':
                                    fieldValue = gridData.data[i][gridData.keyfield];
                                    html += this.CreateButtonEditHtml(columnDef, fieldValue, idx, i, j, l);
                                    break;

                                case 'buttondelete':
                                case 'buttonup':
                                case 'buttondown':
                                    fieldValue = gridData.data[i][gridData.keyfield];
                                    html += this.CreateButtonDeleteHtml(columnDef, fieldValue, idx, i, j, l);
                                    break;

                                case 'thumb':
                                    fieldValue = gridData.data[i][columnDef.field];
                                    html += this.CreateThumbHtml(columnDef, fieldValue, idx, i);
                                    break;

                                case 'enabled':
                                case 'truefalse':
                                    fieldValue = gridData.data[i][columnDef.field];
                                    html += this.CreateEnabledHtml(columnDef, fieldValue, idx, i);
                                    break;

                                case 'ctrlint':
                                    html += this.CreateCtrlIntHtml(columnDef, fieldValue, idx, i, j, l, fldName);
                                    break;

                                case 'ctrldropdownlist':
                                    html += this.CreateCtrlDropDownListHtml(columnDef, fieldValue, idx, i, j, l, fldName);
                                    break;
                            }
                        }
                    }
                    html += '</td>';
                }
            }
            html += '</tr>';
        }

    } else {
        html += '<tr><td colspan="' + gridData.columnDefs.length + '">';
        if (gridData.norecordsmessage) {
            html += gridData.norecordsmessage;
        } else {
            html += 'No records found';
        }
        html += '</td></tr>';
    }

    html += '</tbody>';
    html += '</table>';

    $('#' + gridData.container + 'list').html(html);

    this.RenderPager(idx);
};

GridManager.prototype.ParseWidth = function (width, orderBtnWidth) {
    var rc = parseInt(width) + orderBtnWidth; // can be a number or a number followed by a non-numeric
    var temp = '';
    var i = width.length - 1;
    while (i >= 0 && isNaN(width.substr(i, 1))) {
        temp = width.substr(i, 1) + temp;
        i--;
    }
    rc += temp;
    return rc;
}

GridManager.prototype.CreateTextHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var html = '';
    if (fldConfig.class || fldConfig.style) {
        html = '<span ' + this.CreateClassHtml(fldConfig);
        html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum) + '>';
        html += fieldValue.replace(/\r\n/g, '<br/>');
        html += '</span>';
    } else {
        html = fieldValue;
    }
    return html;
}

GridManager.prototype.CreateFieldHtml = function (fldConfig, fieldValue, data, gridIdx, rowNum, fldName) {
    var html = '<span';
    if (fldName) html += ' id="' + fldName + '"';
    html += this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum) + '>';
    var temp = fieldValue;
    if (fldConfig.type == 'field') {
        temp = this.FormatField(temp, fldConfig);
        if(fldConfig.dontreplacecrlfs != true) temp = temp.replace(/\r\n/g, '<br/>');
    }
    html += temp + '</span>';
    return html;
}

GridManager.prototype.CreateLinkHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var html = '<a href="' + fieldValue + '"' + this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum);
    if (fldConfig.target) html += ' target="' + fldConfig.target + '"';
    html += '>' + fieldValue + "</a>";
    return html;
}

GridManager.prototype.CreateISODateHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var dateStr = '';
    var timeStr = '';
    if (fieldValue != '') {
        var momFormat = fldConfig.format.toUpperCase();     // dd/MM/yyyy
        dateStr = moment(fieldValue).format(momFormat);
        timeStr = moment(fieldValue).format('HH:mm:ss');
    }
    var html = '<span ' + this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum) + '>';

    if (fldConfig.type == 'isodate') html += dateStr;
    if (fldConfig.type == 'isotime') html += timeStr;
    if (fldConfig.type == 'isodatetime') html += dateStr + " " + timeStr;
    if (fldConfig.type == 'isodatetimebr') html += dateStr + "<br/>" + timeStr;

    html += '</span>';
    return html;
}

GridManager.prototype.CreateEMailHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var html = '<a href="mailto:' + fieldValue + '"';
    html += this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum) + '>';
    html += fieldValue + '</a>';
    return html;
}

GridManager.prototype.CreateCheckBoxHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var html = '<input type="checkbox" id="' + fldConfig.id + gridIdx + '_' + rowNum;
    html += '" name="chk' + gridIdx;
    html += '" value="' + fieldValue + '"';
    html += this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum) + '>';
    return html;
}
/*
GridManager.prototype.CreateRadioButtonHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var html = '<input type="radio" id="rad' + (gridIdx + 1) + '" name="rad' + (gridIdx + 1) + '"';
    html += this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum) + '>';
    return html;
}
*/
GridManager.prototype.CreateHiddenHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var html = '<input type="hidden" id="' + fldConfig.id + gridIdx + '_' + rowNum;
    html += '" name="' + fldConfig.id + gridIdx;
    html += '" value="' + fieldValue + '"/>';
    return html;
}

GridManager.prototype.CreateButtonEditHtml = function (fldConfig, fieldValue, gridIdx, rowNum, colNum, fieldNum) {
    var html = '';
    var gridData = this.grids[gridIdx];
    if (fldConfig.type == 'buttonedit' || fldConfig.url) {
        var temp;
        if (fldConfig.type == 'buttonedit') {
            temp = gridData.editurl;
        } else {
            temp = fldConfig.url;
        }
        if (temp) {
            html = '<a href="';
            html += this.DoSubstitutions(temp, rowNum, colNum, fieldNum, fieldValue, gridIdx);
            if (fldConfig.icon) {
                html += '" class="button';
                if (fldConfig.class) html += ' ' + fldConfig.class;
                html += '" onclick="SaveGridState(' + gridIdx + ');" data-toggle="tooltip" data-placement="bottom" title="' + fldConfig.text + '">';
                html += '<span class="glyphicon glyphicon-' + fldConfig.icon + '"></span>';

            } else {
                html += '" class="btn btn-sm';
                if (fldConfig.class) {
                    html += ' ' + fldConfig.class;
                } else {
                    html += ' btn-default';
                }
                html += '" onclick="SaveGridState(' + gridIdx + ');">';
                html += this.FormatField(this.DoSubstitutions(fldConfig.text, rowNum, colNum, fieldNum, fieldValue, gridIdx), fldConfig);
            }
            html += '</a>';
        }

    } else {
        html = '<button type="button" class="';
        if (fldConfig.icon) {
            html += 'button';
            if (fldConfig.class) html += ' ' + fldConfig.class;
        } else {
            html += 'btn btn-sm';
            if (fldConfig.class) {
                html += ' ' + fldConfig.class;
            } else {
                html += ' btn-default';
            }
        }
        html += '"';
        html += ' href="#"';
        html += ' onclick="return ' + this.DoSubstitutions(fldConfig.jsfunc, rowNum, colNum, fieldNum, fieldValue, gridIdx) + '"';
        if (fldConfig.icon) {
            html += ' data-toggle="tooltip" data-placement="bottom" title="' + fldConfig.text + '">';
            html += '<span class="glyphicon glyphicon-' + fldConfig.icon + '"></span>';
        } else {
            html += this.FormatField(this.DoSubstitutions(fldConfig.text, rowNum, colNum, fieldNum, fieldValue, gridIdx), fldConfig);
        }
        html += '</button>';
    }
    if (!fldConfig.icon) html += '&nbsp;';
    return html;
}

GridManager.prototype.CreateButtonDeleteHtml = function (fldConfig, fieldValue, gridIdx, rowNum, colNum, fieldNum) {
    var gridData = this.grids[gridIdx];

    var html = '<button type="button" class="';
    if (fldConfig.icon) {
        html += 'button';
        if (fldConfig.class) html += ' ' + fldConfig.class;
    } else {
        html += 'btn btn-sm';
        if (fldConfig.class) {
            html += ' ' + fldConfig.class;
        } else {
            html += ' btn-default';
        }
    }
    html += '"';

    var temp = gridData.keyfield;
    if (fldConfig.keyfield) gridData.keyfield = fldConfig.keyfield;

    if (fldConfig.type == 'buttondelete') {
        html += ' href="#"';
        html += ' onclick="return GridConfirmDeleteItem(' + gridIdx + ',';  // + gridData.data[rowNum][gridData.keyfield] + ')"';
        html += "'" + this.DoSubstitutions(gridData.deleteurl, rowNum, colNum, fieldNum, gridData.data[rowNum][gridData.keyfield], gridIdx) + "')\"";
    }
    else if (fldConfig.type == 'buttonup') {
        html += ' onclick="GridMoveUpOrDown(';
        html += "'" + this.DoSubstitutions(gridData.moveupurl, rowNum, colNum, fieldNum, gridData.data[rowNum][gridData.keyfield], gridIdx) + "')\"";
    }
    else if (fldConfig.type == 'buttondown') {
        html += ' onclick="GridMoveUpOrDown(';
        html += "'" + this.DoSubstitutions(gridData.movedownurl, rowNum, colNum, fieldNum, gridData.data[rowNum][gridData.keyfield], gridIdx) + "')\"";
    }
    if (fldConfig.icon) {
        html += ' data-toggle="tooltip" data-placement="bottom" title="' + fldConfig.text + '">';
        html += '<span class="glyphicon glyphicon-' + fldConfig.icon + '"></span>';
    } else {
        html += '>' + this.FormatField(this.DoSubstitutions(fldConfig.text, rowNum, colNum, fieldNum, fieldValue, gridIdx), fldConfig);
    }
    gridData.keyfield = temp;;
    html += '</button>';
    if (!fldConfig.icon) html += '&nbsp;';

    return html;
}

GridManager.prototype.GetButtonIcon = function (icon1, icon2) {
    if (icon1) {
        return icon1;
    } else {
        return icon2;
    }
}

GridManager.prototype.CreateThumbHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var html = '<img src="' + fieldValue + '" alt=""';
    html += this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum, 'height:auto') + '/>';
    return html;
}

GridManager.prototype.CreateEnabledHtml = function (fldConfig, fieldValue, gridIdx, rowNum) {
    var trueGraphic = 'Enabled';
    if (fldConfig.truegraphic) trueGraphic = fldConfig.truegraphic.trimRight();
    var falseGraphic = 'Disabled';
    if (fldConfig.falsegraphic) falseGraphic = fldConfig.falsegraphic.trimRight();

    var html = '';
    if (fieldValue == true || fieldValue == 1) {
        if (trueGraphic != '') {
            html += '<img src="/Content/' + trueGraphic + '.png" alt="' + trueGraphic + '"';
            html += this.CreateClassHtml(fldConfig);
            html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum);
            html += ' style="width:25px;"/>';
        }
    } else {
        if (falseGraphic != '') {
            html += '<img src="/Content/' + falseGraphic + '.png" alt="' + falseGraphic + '"';
            html += this.CreateClassHtml(fldConfig);
            html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum);
            html += ' style="width:25px;"/>';
        }
    }
    return html;
}

GridManager.prototype.CreateCtrlIntHtml = function (fldConfig, fieldValue, gridIdx, rowNum, colNum, fieldNum, fldName) {
    var html = '<input type="text" id="' + fldName + '" name="' + fldName + '"';
    html += this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum, fldConfig.style);
    if (fldConfig.maxlength) html += ' maxlength="' + fldConfig.maxlength + '"';
    html += ' value="' + fieldValue + '"';
    if (fldConfig.onchange) html += ' onchange="' + this.DoSubstitutions(fldConfig.onchange, rowNum, colNum, fieldNum, 0, gridIdx) + '"';
    html += '/>';
    return html;
}

GridManager.prototype.CreateCtrlDropDownListHtml = function (fldConfig, fieldValue, gridIdx, rowNum, colNum, fieldNum, fldName) {
    var html = '<select id="' + fldName + '" name="' + fldName + '"';
    html += this.CreateClassHtml(fldConfig);
    html += this.CreateStyleHtml(fldConfig, gridIdx, rowNum, fldConfig.style);
    if (fldConfig.onchange) html += ' onchange="' + this.DoSubstitutions(fldConfig.onchange, rowNum, colNum, fieldNum, 0, gridIdx) + '"';
    html += '>';
    if (fldConfig.list) {
        var items = fldConfig.list.split('|');
        for (var i = 0; i < items.length; i++) {
            var item = items[i].split('=');
            html += '<option value="' + item[1] + '"';
            if (item[i] == fieldValue) html += ' selected';
            html += '>' + item[0] + '</option>';
        }
    }
    html += '</select>';
    return html;
}

GridManager.prototype.CreateClassHtml = function (fldConfig, extraClass) {
    var html = '';
    if (fldConfig.class || extraClass) {
        html = ' class="';
        if (fldConfig.class) html += fldConfig.class;
        if (extraClass) {
            if (fldConfig.class) html += ' ';
            html += extraClass;
        }
        html += '"';
    }
    return html;
}

GridManager.prototype.CreateStyleHtml = function (fldConfig, gridIdx, rowNum, extraStyle) {
    var html = '';
    if (fldConfig.width || fldConfig.colourfield) {
        html = ' style="';
        if (fldConfig.width) html += 'width:' + fldConfig.width + ';';
        if (fldConfig.colourfield) html += 'color:' + gridManager.grids[gridIdx].data[rowNum][fldConfig.colourfield] + ';';
        if (extraStyle) html += ';' + extraStyle + ';';
        html += '"';
    }
    return html;
}

GridManager.prototype.CreateFieldName = function (fldName, rowNum, colNum, fldNum) {
    if (fldName) {
        return fldName + rowNum + '_' + colNum + '_' + fldNum;
    } else {
        return '';
    }
}

GridManager.prototype.GetFieldName = function (idx, rowNum, colNum, fldNum) {
    var rc = this.grids[idx].columnDefs[colNum].fields[fldNum].id + rowNum + '_' + colNum + '_' + fldNum;
    return rc;
}

GridManager.prototype.FormatField = function (txt, fldConfig) {
    var rc = txt;

    if (txt) {
        // Got a non-null field value
        if (fldConfig.format) {
            var num;
            if (fldConfig.format == 'N0') {
                num = parseFloat(txt)
                rc = num.toFixed(0);
            } else if (fldConfig.format == 'N1') {
                num = parseFloat(txt)
                rc = num.toFixed(1);
            } else if (fldConfig.format == 'N2') {
                num = parseFloat(txt)
                rc = num.toFixed(2);
            } else if (fldConfig.format == 'N3') {
                num = parseFloat(txt)
                rc = num.toFixed(3);
            } else if (fldConfig.format == 'N4') {
                num = parseFloat(txt)
                rc = num.toFixed(4);
            } else {
                var format = fldConfig.format;
                var repl = '{' + fldConfig.field + '}';
                if (format.indexOf(repl) != -1) rc = format.replace(repl, txt);
            }
        }
    } else {
        // Null field value
        rc = '';
    }
    return '' + rc;
}

// Render the footer of a grid
GridManager.prototype.RenderFooter = function (idx) {
    var gridData = this.grids[idx];

    var pagerWidth = 12;
    var btnWidth = 0;
    html = '<div class="panel"><div class="panel-body"><div class="row">';
    if (gridData.addurl || gridData.restoreurl) {
        html += '<div class="col-sm-';
        if (gridData.addurl) pagerWidth--;
        if (gridData.restoreurl) pagerWidth -= 2;
        html += '' + (12 - pagerWidth) + '" style="padding:0;">';

        if (gridData.addurl) html += this.CreateAddHtml(idx);
        if (gridData.restoreurl) html+= this.CreateRestoreHtml(idx);
        html += '</div>';
    }

    html += '<div id="' + gridData.container + 'pager" class="col-sm-' + pagerWidth + '">';
    html += '</div></div></div></div>';
    $('#' + gridData.container + 'foot').html(html);
    this.RenderPager(idx);
}

GridManager.prototype.RenderPager = function (idx) {
    var gridData = this.grids[idx];

    var numPages = this.GetNumPages(idx);
    var html = '';
    var numRecs = 0;
    if (gridData.data) numRecs = gridData.data.length;

    html = '<button type="button" class="btn btn-sm btn-light" onclick="GridMoveFirst(' + idx + ')" style="height:30px;width:30px;margin-top:-4px;">&lt;&lt</button>';
    html += '<button type="button" class="btn btn-sm btn-light" onclick="GridMovePrev(' + idx + ')" style="height:30px;width:30px;margin-top:-4px;">&lt</button>';

    var btns = new Array();
    var pgrSize = 8;
    var fst = parseInt((gridData.pagenumber - 1) / pgrSize);
    fst = fst * pgrSize + 1;

    for (var i = fst; i < fst + pgrSize && i <= numPages; i++) {
        html += '<button type="button"';
        if (i == gridData.pagenumber) html += ' class="btn btn-sm btn-primary"'; else html += ' class="btn btn-sm btn-light"'
        html += ' style = "height:30px;width:auto;min-width:30px;margin-top:-4px;"';
        html += ' onclick="GridMoveToPage(' + idx + ',' + i + ')">' + i + '</button>';
    }

    html += '<button type="button" class="btn btn-sm btn-light" onclick="GridMoveNext(' + idx + ')" style="height:30px;width:30px;margin-top:-4px;">&gt</button>';
    html += '<button type="button" class="btn btn-sm btn-light" onclick="GridMoveLast(' + idx + ')" style="height:30px;width:30px;margin-top:-4px;">&gt;&gt</button>&nbsp;&nbsp;&nbsp;';

    var pageSize = this.GetPageSize(idx);
    html += '<select id="ddlPageSize' + idx + '" onchange="GridSearch(' + idx + ')" style="width:50px;height:30px;">';
    for (var j = 0; j < gridData.pagesizes.length; j++) {
        html += '<option value="' + gridData.pagesizes[j] + '"';
        if (gridData.pagesizes[j] == pageSize) html += ' selected';
        html += '>' + gridData.pagesizes[j] + '</option > ';
    }
    html += '</select>&nbsp;&nbsp;&nbsp;';

    html += 'Page ' + gridData.pagenumber + ' of ' + numPages + ' from ' + gridData.totalitems + ' item(s)';

    gridData.pagesize = 0;
    $('#' + gridData.container + 'pager').html(html);
}

GridManager.prototype.GetNumPages = function (idx) {
    var gridData = this.grids[idx];

    var numPages = parseInt(gridData.totalitems / this.GetPageSize(idx));
    if (gridData.totalitems - numPages * this.GetPageSize(idx) > 0) numPages++;
    return numPages;
}

GridManager.prototype.DoSubstitutions = function (text, rowNum, colNum, fieldNum, rowId, idx) {
    var rc = '' + text;
    if (rowNum >= 0) rc = rc.replace(/{ROWNUM}/g, rowNum);
    if (colNum >= 0) rc = rc.replace(/{COLNUM}/g, colNum);
    if (fieldNum >= 0) rc = rc.replace(/{FIELDNUM}/g, fieldNum);
    if (rowId >= 0) rc = rc.replace(/{KEYFIELD}/g, rowId);
    if (idx >= 0 && idx < this.grids.length) {
        rc = rc.replace(/{INDEX}/g, idx);
        rc = rc.replace(/{PAGENO}/g, this.grids[idx].pagenumber);
        rc = rc.replace(/{PAGESIZE}/g, this.GetPageSize(idx));
        rc = rc.replace(/{SEARCH}/g, gridManager.GetSearchString(idx, true));
    }
    rc = rc.replace(/{LOVID}/g, $('#ddlLOVs').val());
    //rc = rc.replace(/{BRANDID}/g, $('#ddlBrands').val());
    rc = rc.replace(/{PARENTID}/g, $('#ParentId').val());

    if (idx >= 0) rc = DoFieldSubstitutions(idx, rowNum, rc);

    return rc;
};



// Pager actions
GridManager.prototype.MoveFirst = function (idx) {
    this.grids[idx].pagenumber = 1;
    SaveGridState(idx);
    DisplayGrid(idx);
};
GridManager.prototype.MovePrev = function (idx) {
    if (this.grids[idx].pagenumber > 1) {
        this.grids[idx].pagenumber--;
        SaveGridState(idx);
        DisplayGrid(idx);
    }
};
GridManager.prototype.MoveNext = function (idx) {
    var gridData = this.grids[idx];
    var numPages = this.GetNumPages(idx);
    if (gridData.pagenumber < numPages) {
        gridData.pagenumber++;
        SaveGridState(idx);
        DisplayGrid(idx);
    }
};
GridManager.prototype.MoveLast = function (idx) {
    var gridData = this.grids[idx];
    var numPages = this.GetNumPages(idx);
    if (gridData.pagenumber < numPages) {
        gridData.pagenumber = numPages;
        SaveGridState(idx);
        DisplayGrid(idx);
    }
};
GridManager.prototype.MoveToPage = function (idx, pageNo) {
    var gridData = this.grids[idx];
    var numPages = parseInt(gridData.totalitems / this.GetPageSize(idx)) + 1;
    if (pageNo >= 1 && pageNo <= numPages) {
        gridData.pagenumber = pageNo;
        SaveGridState(idx);
        DisplayGrid(idx);
    }
};
GridManager.prototype.GetPageSize = function (idx) {
    var pageSize = $('#ddlPageSize' + idx).val();
    if (pageSize) {
        return pageSize;
    } else {
        var gridData = gridManager.grids[idx];
        if (gridData.pagesize) {
            return gridData.pagesize;
        } else {
            return this.grids[idx].pagesizes[0];
        }
    }
};
GridManager.prototype.GetSearchString = function (idx, bEncode) {
    var ss = $('#txtSearch' + idx).val();
    if (ss) {
        if (bEncode) ss = encodeURIComponent(ss);   // Encode all characters as %xx for URL inclusion
        return ss;
    } else {
        return '';
    }
}

// Public methods
function GridSearch(idx, editFld) {
    SaveGridState(idx);

    var gridData = gridManager.grids[idx];
    gridData.pagenumber = 1;

    DisplayGrid(idx);
    if (editFld) $('#' + editFld).focus();
    return false;
}

function GridSort(idx, fldName, sortOrder) {
    //alert('Idx: ' + idx + '\nFldName: ' + fldName + '\nSortOrder: ' + sortOrder);
    var gridData = gridManager.grids[idx];
    gridData.pagenumber = 1;
    gridData.sortcolumn = fldName;
    gridData.sortorder = sortOrder;
    SaveGridState(idx);
    DisplayGrid(idx);
}

function SaveGridState(idx) {
    DisplayProgress()

    var gridData = gridManager.grids[idx];
    SetCookie(gridData.container + ':search', gridManager.GetSearchString(idx), 1);
    SetCookie(gridData.container + ':pageno', gridData.pagenumber, 1);
    SetCookie(gridData.container + ':pagesize', gridManager.GetPageSize(idx), 1);
    SetCookie(gridData.container + ':sortcolumn', gridData.sortcolumn, 1);
    SetCookie(gridData.container + ':sortorder', gridData.sortorder, 1);

    if (gridData.searchfields) {
        for (var i = 0; i < gridData.searchfields.length; i++) {
            var fldName = gridData.container + '_' + gridData.searchfields[i].id;
            var fldValue = $('#' + fldName).val();
            SetCookie(fldName, fldValue, 1);
        }
    }
    return true;
}

function LoadGridState(idx) {
    var gridData = gridManager.grids[idx];
    gridData.pagenumber = GetCookie(gridData.container + ':pageno');
    if (!gridData.pagenumber) gridData.pagenumber = 1;

    gridData.pagesize = GetCookie(gridData.container + ':pagesize');
    if (!gridData.pagesize) gridData.pagesize = gridData.pagesizes[0];
    if (!gridData.pagesize) gridData.pagesize = 30;
    $('#ddlPageSize' + idx).val(gridData.pagesize);

    gridData.sortcolumn = GetCookie(gridData.container + ':sortcolumn');
    if (!gridData.sortcolumn) gridData.sortcolumn = gridData.columnDefs[0].fields[0].field;    // Default to first column

    gridData.sortorder = GetCookie(gridData.container + ':sortorder');
    if (!gridData.sortorder) gridData.sortorder = 0;  // 0=ASC,1=DESC

    return true;
}

function DisplayGrid(idx) {
    DisplayProgress();
    var gridData = gridManager.grids[idx];
    var pageNo = gridData.pagenumber;
    var pageSize = gridManager.GetPageSize(idx);
    var tempUrl = gridData.datasourceurl.replace(/{INDEX}/g, idx);
    tempUrl = tempUrl.replace(/{PAGENO}/g, pageNo);
    tempUrl = tempUrl.replace(/{PAGESIZE}/g, pageSize);
    tempUrl = tempUrl.replace(/{SEARCH}/g, gridManager.GetSearchString(idx, true));

    tempUrl = tempUrl.replace(/{LOVID}/g, $('#ddlLOVs').val());
    tempUrl = tempUrl.replace(/{BRANDID}/g, $('#ddlBrands').val());
    tempUrl = tempUrl.replace(/{PARENTID}/g, $('#ParentId').val());

    tempUrl = DoFieldSubstitutions(idx, -1, tempUrl);

    if (gridData.sortcolumns) {
        tempUrl += '&sortcolumn=' + gridData.sortcolumn;
        tempUrl += '&sortorder=';
        if (gridData.sortorder == 0 || gridData.sortorder == 1) {
            tempUrl += gridData.sortorder;
        } else {
            tempUrl += '0';
        }
    } else {
        tempUrl += '&sortcolumn=&sortorder=0';
    }
    tempUrl += '&tz=' + tz;             // Time zone offset
    tempUrl = AppendQSDate(tempUrl);    // Stops browser caching issues

    $.ajax({
        url: tempUrl
    }).done(function (data) {
        if (data.Error) {
            if (data.Error.Message) {
                DisplayError(data.Error.Icon, data.Error.Message);
            } else {
                var idx = data.GridIndex;
                gridManager.grids[idx].totalitems = data.TotalRecords;
                gridManager.grids[idx].data = data.Items;
                gridManager.RenderHeader(idx);
                gridManager.RenderList(idx);
                if (gridManager.grids[idx].clearmessageondisplay) HideError();
                if (gridManager.grids[idx].ondisplaycomplete) gridManager.grids[idx].ondisplaycomplete();
                $('[data-toggle="tooltip"]').tooltip();
                HideProgress();
            }
        } else {
            DisplayExclamationError('The webservice returned an error to the grid: ' + data);
        }
    }).fail(function (jqXhr, status, error) {
        DisplayExclamationError('The webservice returned an error to the grid: ' + status + ' : ' + error);
    });
}

function DoFieldSubstitutions(idx, rowNum, str) {
    var tempStr = str;

    if (tempStr) {
        var gridData = gridManager.grids[idx];

        // Firstly, handle substituting screen field control values
        var pos1 = tempStr.indexOf('{');
        while (pos1 != -1) {
            var pos2 = tempStr.indexOf('}', pos1 + 1);
            var varName;
            var fullVarName;
            if (pos2 == -1) {
                pos1 = -1;  // Terminate the loop - can't fix missing }

            } else {
                varName = tempStr.substr(pos1 + 1, (pos2 - pos1) - 1);
                fullVarName = gridData.container + '_' + varName;
                var varValue = $('#' + fullVarName).val();
                if (!varValue) varValue = '0';
                tempStr = tempStr.substr(0, pos1) + varValue + tempStr.substr(pos2 + 1);

                pos1 = tempStr.indexOf('{');
            }
        }

        // Now handle record field values
        if (gridData.data) {
            var pos1 = tempStr.indexOf('[');
            while (pos1 != -1) {
                var pos2 = tempStr.indexOf(']', pos1 + 1);
                var varName;
                var fullVarName;
                if (pos2 == -1) {
                    pos1 = -1;  // Terminate the loop - can't fix missing ]

                } else {
                    varName = tempStr.substr(pos1 + 1, (pos2 - pos1) - 1);
                    var varValue = gridData.data[rowNum][varName];
                    if (!varValue) varValue = '';
                    tempStr = tempStr.substr(0, pos1) + varValue + tempStr.substr(pos2 + 1);

                    pos1 = tempStr.indexOf('[');
                }
            }
        }
    }

    return tempStr;
}

function GridMoveFirst(idx) {
    gridManager.MoveFirst(idx);
}
function GridMovePrev(idx) {
    gridManager.MovePrev(idx);
}
function GridMoveNext(idx) {
    gridManager.MoveNext(idx);
}
function GridMoveLast(idx) {
    gridManager.MoveLast(idx);
}
function GridMoveToPage(idx, pageNo) {
    gridManager.MoveToPage(idx, pageNo);
}

function GridMoveUpOrDown(tempUrl) {
    $.ajax({
        url: AppendQSDate(tempUrl)      // Stops browser caching issues
    }).done(function (data) {
        if (data.Error.Message) {
            DisplayError(data.Error.Icon, data.Error.Message);
        } else {
            var idx = data.GridIndex;
            gridManager.grids[idx].totalitems = data.TotalRecords;
            gridManager.grids[idx].data = data.Items;
            gridManager.RenderList(idx);
            HideProgress();
        }
    }).fail(function (jqXhr, status, error) {
        DisplayExclamationError('The webservice returned an error to the grid: ' + status + ' : ' + error);
    });
}

function GridConfirmDeleteItem(idx, url) {
    return Confirm3(gridManager.grids[idx].deleteconfirm, gridManager.grids[idx].appname, idx, 'GridDoDelete', url);
}
function GridDoDelete(idx, url) {
    DisplayProgress();

    var cont = true;
    var gridData = gridManager.grids[idx];
    if (gridData.deletecallback) cont = gridData.deletecallback(url);

    if (cont) {
        var tempUrl = url;

        tempUrl += '&tz=' + tz;                 // Time zone offset
        tempUrl = AppendQSDate(tempUrl);       // Stops browser caching issues

        $.ajax({
            url: tempUrl
        })
        .done(function (data, status, xhr) {
            if (data.Error.Message) {
                DisplayError(data.Error.Icon, data.Error.Message);
            } else {
                var gridData = gridManager.grids[data.GridIndex];
                if (gridData.deletesuccess) gridData.deletesuccess();
                DisplayGrid(data.GridIndex);
            }
        })
        .fail(function (xhr, status, error) {
            HideProgress();
            DisplayExclamationError('The webservice returned an error to the grid: ' + status + ' : ' + error);
        });
    }
    HideProgress();
}

function GridConfirmRestoreDefaults(idx) {
    return Confirm2(gridManager.grids[idx].restoreconfirm, gridManager.grids[idx].appname, idx, 0, 'GridDoRestoreDefaults');
}
function GridDoRestoreDefaults(idx, id) {
    DisplayProgress();
    var tempUrl = '' + gridManager.grids[idx].restoreurl;
    tempUrl = tempUrl.replace(/{INDEX}/g, idx);
    tempUrl = tempUrl.replace(/{KEYFIELD}/g, id);
    tempUrl = tempUrl.replace(/{LOVID}/g, $('#ddlLOVs').val());

    tempUrl = DoFieldSubstitutions(idx, -1, tempUrl);

    tempUrl += '&tz=' + tz;                 // Time zone offset
    tempUrl = AppendQSDate(tempUrl);       // Stops browser caching issues

    $.ajax({
        url: tempUrl
    })
    .done(function (data, status, xhr) {
        if (data.Error.Message) {
            DisplayError(data.Error.Icon, data.Error.Message);
        } else {
            DisplayGrid(data.GridIndex);
        }
    })
    .fail(function (xhr, status, error) {
        HideProgress();
        DisplayExclamationError('The webservice returned an error to the grid: ' + status + ' : ' + error);
    });
}

function DoAjaxCall(url) {
    DisplayProgress();
    var result = '';
    var tempUrl = AppendQSDate(url);     // Stops browser caching issues
    $.ajax({
        url: tempUrl,
        async: false
    })
    .done(function (data, status, xhr) {
        HideProgress();
        HideError();
        if (data.Error) {
            // New style JSONResult
            DisplayJSONResult(data);
        }
        result = data;
    })
    .fail(function (xhr, status, error) {
        HideProgress();
        DisplayExclamationError('The webservice returned an error: ' + status + ' : ' + error);
    });
    return result;
}

function DisplayJSONResult(data) {
    if (data.Error.Message != '') {
        if (data.Error.IsError) {
            DisplayExclamationError(data.Error.Message);
        } else {
            DisplayInformationError(data.Error.Message);
        }
    }
}

function AppendQSDate(url) {
    var tempUrl = url;
    if (tempUrl.indexOf('&d=') == -1 &&
        tempUrl.indexOf('?d=') == -1) {
        if (tempUrl.indexOf('?') == -1) {
            tempUrl += '?';
        } else {
            tempUrl += '&';
        }
        tempUrl += 'd=' + new Date().getTime();
    }
    return tempUrl;
}

function GetValueFromUrl(url, key) {
    var rc = '';
    if (url) {
        var temp = url;
        var pos = temp.indexOf('?');
        if (pos != -1) temp = temp.substr(pos + 1);
        var pairs = temp.split('&');
        for (var i = 0; i < pairs.length; i++) {
            var pair = pairs[i].split('=');
            if (pair[0] == key) {
                rc = pair[1];
                i = pairs.length;
            }
        }
    }
    return rc;
}

function GetSelectedItems(idx, selectId, hdnId) {
    var selected = '';
    var gridData = gridManager.grids[idx];

    // chk1_0
    var sel = $('#' + gridData.container + ' input:checked');
    for (var i = 0; i < sel.length; i++) {
        var hdn = sel[i].id;   // chk1_1
        hdn = hdn.replace(selectId, hdnId);
        if (selected != '') selected += ',';
        selected += $('#' + hdn).val();
    }
    return selected;
}