var upCallbackFunc = null;
var selectCallbackFunc = null;
var signalLoadCallBack = null;
var splitSize = 2000;
//$.connection.hub.logging = true;
$.connection.hub.url = "http://localhost:54321/signalr";
var chat = $.connection.mttHub;

var signalFunc = null;
var signalParam1 = null;
var signalParam2 = null;

function SetInfoPosition() {
    if ($("#mttInfo").css("position") != "fixed") {
        $("#mttInfo").css("position", "absolute");
        var dw = $(window).width();
        var ow = $("#mttInfo").outerWidth();
        var dh = $(window).height();
        var oh = $("#mttInfo").outerHeight();
        var l = (dw - ow) / 2;
        var t = (dh - oh) / 2 > 0 ? (dh - oh) / 2 : 10;
        var lDiff = $("#mttInfo").offset().left - $("#mttInfo").position().left;
        var tDiff = $("#mttInfo").offset().top - $("#mttInfo").position().top;
        l = l + $(window).scrollLeft() - lDiff;
        t = t + $(window).scrollTop() - tDiff;
        $("#mttInfo").css("left", l + "px");
        $("#mttInfo").css("top", t + "px");
    }
}

var ifr;
function unInstall() {
    CheckMttInstall();
    setTimeout(function () {
        if ($.connection.hub.state == 4) {
            $.signalR.ajaxDefaults.timeout = 2000;
            $.connection.hub.start({ jsonp: true }).done(function () {
                if ($.connection.hub.state == 1) {
                    signalFunc(true, signalParam1, signalParam2);
                }
            }).fail(function () {
                if ($("#mttInfo").length == 0) {
                    $(document.body).append('<div class="yh-Mtt" id="mttInfo"><div class="yh-Mtt-tit"><div class="yh-Mtt-tit-name">MTT飞象</div><div class="yh-Mtt-tit-close" onclick="$(\'#mttInfo\').remove();"><i class="yh-Mtt-tit-close-img"></i></div></div><div class="yh-Mtt-con"><a href="/webMT/app/newInstall/MTTSetup.exe" class="yh-Mtt-btn_start">下载客户端</a><span class="yh-Mtt-link">提示：若已安装MTT飞象，请启动MTT！</span></div></div>');
                    SetInfoPosition();
                }
            });
            $.signalR.ajaxDefaults.timeout = null;
        }
        ifr.parentNode.removeChild(ifr);
    }, 1000);
}
function CheckMttInstall() {
    ifr = document.createElement('iframe');
    ifr.src = "mtt://openClient";
    ifr.style.display = 'none';
    $('body').append(ifr);
}


function customProtocolClick() {
    $("#mttInfo").remove();
}

chat.client.SendMessage=function(){
    signalFunc(true, signalParam1,signalParam2);
}

chat.client.MttUnOpen = function (id) {
    var href = "mtt://" + id;
    if ($("#mttInfo").length == 0) {
        $(document.body).append('<iframe class="yh-Mtt" id="mttInfo" src="' + href + '" style="display:none;"></iframe>');
    }
    else {
        $("#mttInfo").attr("src", href);
    }
}

chat.client.CloseSignalR = function () {
    $.connection.hub.stop();
}

chat.client.SelectFileCallback = function (fileList) {
    selectCallbackFunc(fileList);
    uploaded = true;
};

function SelectFile(recall, isMulti, rejectTypes) {
    if (recall != true)
    {
        signalFunc = SelectFile;
        signalParam1 = isMulti;
        signalParam2 = rejectTypes;
    }
    if ($.connection.hub.state == 4) {
        $.signalR.ajaxDefaults.timeout = 2000;
        $.connection.hub.start({ jsonp: true }).done(function () {
            if ($.connection.hub.state != 1)
            {
                SelectFile(false,isMulti);
                return;
            }
            chat.server.openMTTClientUpload(isMulti,rejectTypes);
        }).fail(function () {
            unInstall();
        });
        $.signalR.ajaxDefaults.timeout = null;
    } else if ($.connection.hub.state == 1) {
        chat.server.openMTTClientUpload(isMulti, rejectTypes);
    }
}

function ClientUploadFile(recall, files) {
    if (files == undefined && recall)
    {
        ClientUploadFile(false, recall);
        return;
    }
    if (recall != true) {
        signalFunc = ClientUploadFile;
        signalParam1 = files;
        signalParam2 = null;
    }
    var fileInfoStr = "";
    var i = 0;
    for (; i < files.length; i++) {
        fileInfoStr = fileInfoStr + "|H|" + files[i].path + "|-|" + files[i].strParam;
    }
    fileInfoStr = encodeURIComponent(fileInfoStr);
    var key = Math.random();
    if ($.connection.hub.ajaxDataType === "jsonp") {
        var num = Math.ceil(fileInfoStr.length / splitSize);
        for (var j = 0; j < num; j++) {
            var tempStr = fileInfoStr.substring(j * splitSize, j * splitSize + splitSize);
            chat.server.startUpload(masterAddress, bizGuid, tempStr, key, j + 1, num);
        }
    } else {
        chat.server.startUpload(masterAddress, bizGuid, fileInfoStr, key, 1, 1);
    }
}

function ClientDownloadFile(guid, id, version, name, ext, size) {
    var downlist = [];
    downlist.push({ guid: guid, id: id, version: version, name: name, ext: ext, size: size, path: "" });
    ClientDownloadFiles(false, downlist)
}

function ClientDownloadFiles(recall, downlist) {
    if (downlist == undefined && recall) {
        ClientDownloadFiles(false, recall);
        return;
    }
    if (recall != true) {
        signalFunc = ClientDownloadFiles;
        signalParam1 = downlist;
        signalParam2 = null;
    }
    var fileInfoStr = "";
    var fileIds = "";
    $(downlist).each(function () {
        if (this.guid != "") {
            fileInfoStr = fileInfoStr + "|*|" + this.guid + "|-|" + this.path + "|-|" + this.name + "|-|" + this.ext + "|-|" + this.size;
        }
        if (this.path == "") {
            fileIds += "," + this.id + "|" + this.version;
        }
    });
    if (fileInfoStr == "") {
        //$.tmsg("m_jfw", "文件尚未上传完成，请稍后刷新页面下载！", { infotype: 2, time_out: 1000 });
        layer.msg("文件尚未上传完成，请稍后刷新页面下载！", { icon: 5 });
        return;
    }
    fileInfoStr = encodeURIComponent(fileInfoStr);
    var key = Math.random();
    var num = Math.ceil(fileInfoStr.length / splitSize);
    if ($.connection.hub.state == 4) {
        $.signalR.ajaxDefaults.timeout = 2000;
        $.connection.hub.start({ jsonp: true }).done(function () {
            if ($.connection.hub.state != 1)
            {
                ClientDownloadFiles(false, downlist);
                return;
            }
            if ($.connection.hub.ajaxDataType === "jsonp") {
                for (var j = 0; j < num; j++) {
                    var tempStr = fileInfoStr.substring(j * splitSize, j * splitSize + splitSize);
                    chat.server.startDownload(masterAddress, bizGuid, tempStr, key, j + 1, num);
                }
            } else {
                chat.server.startDownload(masterAddress, bizGuid, fileInfoStr, key, 1, 1);
            }
            if (fileIds != "") {
                FileDownloadCount(fileIds);
            }
        }).fail(function () {
            unInstall();
        });
        //alert(3)
        $.signalR.ajaxDefaults.timeout = null;
    } else if ($.connection.hub.state == 1) {
        if ($.connection.hub.ajaxDataType === "jsonp") {
            for (var j = 0; j < num; j++) {
                var tempStr = fileInfoStr.substring(j * splitSize, j * splitSize + splitSize);
                chat.server.startDownload(masterAddress, bizGuid, tempStr, key, j + 1, num);
            }
        } else {
            chat.server.startDownload(masterAddress, bizGuid, fileInfoStr, key, 1, 1);
        }
        if (fileIds != "") {
            FileDownloadCount(fileIds);
        }
        //alert(4)
    }
}


function ReadPdfFile(recall, path, fileName) {
    if (fileName == undefined && recall&&path) {
        ReadPdfFile(false, recall, path);
    }
    if (recall == false) {
        signalFunc = ReadPdfFile;
        signalParam1 = path;
        signalParam2 = fileName;
    }
    if ($.connection.hub.state == 4) {
        $.signalR.ajaxDefaults.timeout = 2000;
        $.connection.hub.start({ jsonp: true }).done(function () {
            if ($.connection.hub.state != 1) {
                ReadPdfFile(false,path,fileName);
                return;
            }
            chat.server.readPdfFile(encodeURI(path),fileName);
        }).fail(function () {
            unInstall();
        });
        $.signalR.ajaxDefaults.timeout = null;
    } else if ($.connection.hub.state == 1) {
        chat.server.readPdfFile(encodeURI(path),fileName);
    }
}
function ReadExcelFile(recall, path, fileName) {
    if (fileName == undefined && recall && path) {
        ReadExcelFile(false, recall, path);
    }
    if (recall == false) {
        signalFunc = ReadExcelFile;
        signalParam1 = path;
        signalParam2 = fileName;
    }
    if ($.connection.hub.state == 4) {
        $.signalR.ajaxDefaults.timeout = 2000;
        $.connection.hub.start({ jsonp: true }).done(function () {
            if ($.connection.hub.state != 1) {
                ReadExcelFile(false, path, fileName);
                return;
            }
            chat.server.readExcelFile(encodeURI(path), fileName);
        }).fail(function () {
            unInstall();
        });
        $.signalR.ajaxDefaults.timeout = null;
    } else if ($.connection.hub.state == 1) {
        chat.server.readExcelFile(encodeURI(path), fileName);
    }
}
function ReadDwgFile(recall, path, fileName) {
    if (fileName == undefined && recall && path) {
        ReadDwgFile(false, recall, path);
    }
    if (recall == false) {
        signalFunc = ReadDwgFile;
        signalParam1 = path;
        signalParam2 = fileName;
    }
    if ($.connection.hub.state == 4) {
        $.signalR.ajaxDefaults.timeout = 2000;
        $.connection.hub.start({ jsonp: true }).done(function () {
            if ($.connection.hub.state != 1) {
                ReadDwgFile(false, path, fileName);
                return;
            }
            chat.server.readDwgFile(encodeURI(path),fileName);
        }).fail(function () {
            unInstall();
        });
        $.signalR.ajaxDefaults.timeout = null;
    } else if ($.connection.hub.state == 1) {
        chat.server.readDwgFile(encodeURI(path),fileName);
    }
}


function ReadVedioFile( path, fileName) {
    $("#p-j-vediocontent").show();
    var url = "/Attachment/FlowPalyerVideoShow?videoUrl=" + encodeURI(path) + "&title=" + encodeURI(fileName) +"&isPop=1" + "&r=" + Math.random();
    $("#p-j-vediocontent").empty().load(url, function () {
    });
}

function UploadFiles(o, isMultiply, callbackFunc, rejectTypes, override) {
    upCallbackFunc = callbackFunc;
    selectCallbackFunc = SelectFileCallback;
    if ($(o).attr("fileObjId")) {
        $("#fileObjId").val($(o).attr("fileObjId"));
    }
    if ($(o).attr("fileTypeId")) {
        $("#fileTypeId").val($(o).attr("fileTypeId"));
    }
    if ($(o).attr("keyValue")) {
        $("#keyValue").val($(o).attr("keyValue"));
    }
    if ($(o).attr("tableName")) {
        $("#tableName").val($(o).attr("tableName"));
    }
    if ($(o).attr("keyName")) {
        $("#keyName").val($(o).attr("keyName"));
    }
    if ($(o).attr("uploadType")) {
        $("#uploadType").val($(o).attr("uploadType"));
    }
    if ($(o).attr("fileRel_profId")) {
        if (typeof $("#fileRel_profId").val() != "undefined") {
            $("#fileRel_profId").val($(o).attr("fileRel_profId"));
        }
    }
    if ($(o).attr("fileRel_stageId")) {
        if (typeof $("#fileRel_stageId").val() != "undefined") {
            $("#fileRel_stageId").val($(o).attr("fileRel_stageId"));
        }
    }
    if ($(o).attr("fileRel_fileCatId")) {
        if (typeof $("#fileRel_fileCatId").val() != "undefined") {
            $("#fileRel_fileCatId").val($(o).attr("fileRel_fileCatId"));
        }
    }
    if (isMultiply == "true") {
        if (rejectTypes != null && rejectTypes != undefined) {
            if (override == true) {
                SelectFile(false, true, rejectTypes);
            } else {
                SelectFile(false, true, mttRejectTypes + "," + rejectTypes);
            }
        } else {
            SelectFile(false, true, mttRejectTypes);
        }
    }
    else if (isMultiply == "false") {
        if (rejectTypes != null && rejectTypes != undefined) {
            if (override == true) {
                SelectFile(false, false, rejectTypes);
            } else {
                SelectFile(false, false, mttRejectTypes + "," + rejectTypes);
            }
        } else {
            SelectFile(false, false, mttRejectTypes);
        }
    }
}

function SelectFileCallback(fileList) {
    var propertyStr = "&";
    var fileObjId = $("#fileObjId").val();
    var fileTypeId = $("#fileTypeId").val();
    var keyValue = $("#keyValue").val();
    var tableName = $("#tableName").val();
    var keyName = $("#keyName").val();
    var fileRel_profId = $("#fileRel_profId").val();
    var fileRel_stageId = $("#fileRel_stageId").val();
    var fileRel_fileCatId = $("#fileRel_fileCatId").val();
    var uploadType = "";

    if ($("#uploadType")) {
        uploadType = $("#uploadType").val();
    }

    $("input[name^='Property_']").each(function (i, val) {
        var prop = $(this).attr("name") + "=" + $(this).attr("value") + "&";
        propertyStr += prop;
    });

    var d = fileList
    var i = 0;
    var len = d.length;
    var str = "";
    var o;

    var NewUserFilePath = "";

    var splitStr = "|H|";

    for (; i < len; ++i) {
        o = d[i];
        if (NewUserFilePath == "") {
            NewUserFilePath = o.NativePath;
        }
        else {
            NewUserFilePath += splitStr + o.NativePath;
        }
        NewUserFilePath += ("|Y|" + o.RootDir);

    }
    var dataStr = "fileObjId=" + fileObjId;
    dataStr += "&fileTypeId=" + fileTypeId;
    dataStr += "&uploadFileList=" + encodeURIComponent(NewUserFilePath);
    dataStr += "&tableName=" + tableName;
    dataStr += "&keyName=" + keyName;
    dataStr += "&keyValue=" + keyValue;
    dataStr += "&uploadType=" + uploadType;
    dataStr += "&fileRel_profId=" + fileRel_profId;
    dataStr += "&fileRel_stageId=" + fileRel_stageId;
    dataStr += "&fileRel_fileCatId=" + fileRel_fileCatId;
    $("input[fileattributes=true]").each(function () {
        if ($(this).val() != null) {
            dataStr += "&" + $(this).attr("name") + "=" + $(this).val();
            $(this).val(null);
        }
    });
    var arr;
    $("body").maskInfo({ loadType: "10" });
    $.ajax({
        url: '/FileLib/SaveMultipleUploadFiles',
        type: 'post',
        data: dataStr + propertyStr,
        dataType: 'html',
        error: function () {
            $("body").unmaskInfo();
            alert("添加失败");
        },
        success: function (data) {
            $("body").unmaskInfo();
            var str = data.split("|");
            var result = eval("(" + str[0] + ")");
            var fileIdList = "";
            if (result.success) {
                if (str.length > 1) {
                    if (str[1] != "") {
                        var files = eval("(" + str[1] + ")");
                        ClientUploadFile(false,files);
                        
                        if (upCallbackFunc != null) {
                            upCallbackFunc();
                            upCallbackFunc = null;
                        }
                        else {
                            setTimeout("window.location.reload();", 50) 
                        }
                    }
                }
            }
        }
    });
}

function DeleteFiles(ids, func, param) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复！")) {
        //$("body").maskInfo({ loadType: "10" });
        $.ajax({
            url: "/Home/DeleFiles",
            type: 'post',
            data: { delFileRelIds: ids },
            dataType: 'json',
            error: function () {
                //$("body").unmaskInfo();
                alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
            },
            success: function (data) {
                //$("body").unmaskInfo();
                if (data.Success == false) {
                    alert(data.msgError, '删除失败');
                }
                else {
                    if (func) {
                        if (typeof param != "undefined") {
                            func(param);
                        } else {
                            func();
                        }
                    } else {
                        window.location.reload();
                    }
                }
            }
        });
    }

}

function UploadFilesAndSave(o, isMultiply, rejectTypes, override) {
    selectCallbackFunc = SelectFileCallbackAndSave;
    $("#fileObjId").val($(o).attr("fileObjId"));
    $("#fileTypeId").val($(o).attr("fileTypeId"));
    $("#keyValue").val($(o).attr("keyValue"));
    $("#tableName").val($(o).attr("tableName"));
    $("#keyName").val($(o).attr("keyName"));
    if (isMultiply == "true") {
        if (rejectTypes != null && rejectTypes != undefined) {
            if (override == true) {
                SelectFile(false, true, rejectTypes);
            } else {
                SelectFile(false, true, mttRejectTypes + "," + rejectTypes);
            }
        } else {
            SelectFile(false, true, mttRejectTypes);
        }
    }
    else if (isMultiply == "false") {
        if (rejectTypes != null && rejectTypes != undefined) {
            if (override == true) {
                SelectFile(false, false, rejectTypes);
            } else {
                SelectFile(false, false, mttRejectTypes + "," + rejectTypes);
            }
        } else {
            SelectFile(false, false, mttRejectTypes);
        }
    }
}

function SelectFileCallbackAndSave(fileList) {  //
    var fileObjId = $("#fileObjId").val();
    var fileTypeId = $("#fileTypeId").val();
    var keyValue = $("#keyValue").val();
    var tableName = $("#tableName").val();
    var keyName = $("#keyName").val();
    var d = fileList;
    var i = 0;
    var len = d.length;
    var str = "";
    var o;

    var NewUserFilePath = "";

    var splitStr = "|H|";

    for (; i < len; ++i) {
        o = d[i];
        if (NewUserFilePath == "") {
            NewUserFilePath = o.NativePath;
        }
        else {
            NewUserFilePath += splitStr + o.NativePath;
        }
        NewUserFilePath += ("|Y|" + o.RootDir);

    }
    $("#uploadFileList").val(NewUserFilePath);
    var html = "";
    var a = NewUserFilePath.split("|H|");
    for (var i = 0; i < d.length; i++) {
        var fileName = "";
        var fileExt = "";
        fileName = d[i].Name;
        fileExt = d[i].Type || '';
        fileName = fileName.replace(fileExt, '');
        html += '<tr>';
        html += '<td><div class="tb_con">' + fileName;
        html += '</div></td>';
        html += '<td><div class="tb_con">' + fileExt;
        html += '</div></td>';
        html += '<td><div class="tb_con">' + d[i].FileSizeCalc;
        html += '</div></td>';
        html += '<td><div class="tb_con">';
        html += '</div></td>'
        html += '</tr>';
        //}


    }
    var AppendTable = $(".table01:visible:first");
    if (fileTypeId != 0) {
        AppendTable = $("table[filetypeid=" + fileTypeId + "]");
    }
    AppendTable.find("tbody").html(html);
}

function DeleteFilesAndSave(fileRetId, o) {
    var delIds = $("#delFileRelIds").val();
    delIds = delIds + fileRetId + ",";
    $("#delFileRelIds").val(delIds);
    $(o).parent().parent().parent().remove();
}

function DeleteFolderByStructId(structId, callback, param) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复！")) {
        $("body").maskInfo({ loadType: "10" });
        $.ajax({
            url: "/FileLib/DeleFolder",
            type: 'post',
            data: { delstructIds: structId },
            dataType: 'json',
            error: function () {
                $("body").unmaskInfo();
                alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
            },
            success: function (data) {
                $("body").unmaskInfo();
                if (data.Success == false) {
                    alert(data.Message, '删除失败');
                }
                else {
                    if (typeof callback == "function") {
                        if (typeof param != "undefined") {
                            callback(param);
                        } else {
                            callback();
                        }
                        
                    } else {
                        window.location.reload();
                    }
                }
            }
        });
    }
}

function DeleteFileByFileId(fileId, func) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复！")) {
        $("body").maskInfo({ loadType: "10" });
        $.ajax({
            url: "/FileLib/DeleFilesByFileId",
            type: 'post',
            data: { delFileIds: fileId },
            dataType: 'json',
            error: function () {
                $("body").unmaskInfo();
                alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
            },
            success: function (data) {
                $("body").unmaskInfo();
                if (data.Success == false) {
                    alert(data.Message, '删除失败');
                }
                else {
                    if (func) {
                        func();
                    } else {
                        window.location.reload();
                    }
                }
            }
        });
    }
}

function DeleteFileByFileId(fileId, func, param) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复！")) {
        $("body").maskInfo({ loadType: "10" });
        $.ajax({
            url: "/FileLib/DeleFilesByFileId",
            type: 'post',
            data: { delFileIds: fileId },
            dataType: 'json',
            error: function () {
                $("body").unmaskInfo();
                alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
            },
            success: function (data) {
                $("body").unmaskInfo();
                if (data.Success == false) {
                    alert(data.Message, '删除失败');
                }
                else {
                    if (func) {
                        if (typeof param != "undefined") {
                            func(param);
                        } else {
                            func();
                        }
                    } else {
                        window.location.reload();
                    }
                }
            }
        });
    }
}

//function Listdown() {
//    if ($("input[name=downList]:checked").length == 0) { alert("请选择需要批量下载的文件!"); return false; }
//    var downlist = [];
//    $("input[name=downList]:checked").each(function () {
//        var name, guId;
//        name = $(this).attr("fname");
//        guId = $(this).attr("guid");
//        fileId = $(this).attr("fileid");
//        downlist.push({ guid: guId, name: name, fileId: fileId });
//    });
//    if (downlist.length) {
//        cwf.downLoadBatchFile(downlist, "");
//    }
//}

function DeleteFileByFileIds(ids) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复！")) {
        $("body").maskInfo({ loadType: "10" });
        $.ajax({
            url: "/FileLib/DeleFilesByFileId",
            type: 'post',
            data: { delFileIds: ids },
            dataType: 'json',
            error: function () {
                $("body").unmaskInfo();
                alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
            },
            success: function (data) {
                $("body").unmaskInfo();
                if (data.Success == false) {
                    alert(data.Message, '删除失败');
                }
                else {
                    window.location.reload();
                }
            }
        });
    }
}
function DeleteFileByFileIdsWidthCallBack(ids, callback) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复！")) {
        $("body").maskInfo({ loadType: "10" });
        $.ajax({
            url: "/FileLib/DeleFilesByFileId",
            type: 'post',
            data: { delFileIds: ids },
            dataType: 'json',
            error: function () {
                $("body").unmaskInfo();
                alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
            },
            success: function (data) {
                $("body").unmaskInfo();
                if (data.Success == false) {
                    alert(data.Message, '删除失败');
                }
                else {
                    if (typeof callback == "function") {
                        callback();
                    } else {
                        window.location.reload();
                    }
                }
            }
        });
    }
}

function DeleteFileVersionByFileVerIds(ids) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复！")) {
        $("body").maskInfo({ loadType: "10" });
        $.ajax({
            url: "/FileLib/DeleFilesByFileVerId",
            type: 'post',
            data: { delFileIds: ids },
            dataType: 'json',
            error: function () {
                $("body").unmaskInfo();
                alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
            },
            success: function (data) {
                $("body").unmaskInfo();
                if (data.Success == false) {
                    alert(data.Message, '删除失败');
                }
                else {
                    window.location.reload();
                }
            }
        });
    }
}

function DeleteFileVersionByFileVerIds(ids, func, param) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复！")) {
        $("body").maskInfo({ loadType: "10" });
        $.ajax({
            url: "/FileLib/DeleFilesByFileVerId",
            type: 'post',
            data: { delFileIds: ids },
            dataType: 'json',
            error: function () {
                $("body").unmaskInfo();
                alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
            },
            success: function (data) {
                $("body").unmaskInfo();
                if (data.Success == false) {
                    alert(data.Message, '删除失败');
                }
                else {
                    if (func) {
                        if (typeof param != "undefined") {
                            func(param);
                        } else {
                            func();
                        }
                    } else {
                        window.location.reload();
                    }
                }
            }
        });
    }
}
function UploadNewVersionFiles(o, callBackFunc, rejectTypes, override) {

    if ($(o).attr("fileId")) {
        $("#fileId").val($(o).attr("fileId"));
    }
    upCallbackFunc = callBackFunc;
    selectCallbackFunc = SelectFileNewVersionCallback;
    if (rejectTypes != null && rejectTypes != undefined) {
        if (override == true) {
            SelectFile(false, false, rejectTypes);
        } else {
            SelectFile(false, false, mttRejectTypes + "," + rejectTypes);
        }
    } else {
        SelectFile(false, false, mttRejectTypes);
    }
}

function SelectFileNewVersionCallback(file) {

    var fileId = $("#fileId").val();

    var d = file;

    var i = 0;
    var len = d.length;
    var str = "";
    var o;

    var NewUserFilePath = "";

    var splitStr = "|H|";

    for (; i < len; ++i) {
        o = d[i];
        if (NewUserFilePath == "") {
            NewUserFilePath = o.NativePath;
        }
        else {
            NewUserFilePath += splitStr + o.NativePath;
        }


    }

    var dataStr = "fileId=" + fileId;
    dataStr += "&uploadFileList=" + encodeURIComponent(NewUserFilePath);

    var arr;
    $("body").maskInfo({ loadType: "10" });
    $.ajax({
        url: '/FileLib/SaveNewVersion',
        type: 'post',
        data: dataStr,
        dataType: 'html',
        error: function () {
            $("body").unmaskInfo();
            alert("添加失败");

        },
        success: function (data) {
            $("body").unmaskInfo();
            var str = data.split("|");
            var result = eval("(" + str[0] + ")");
            var fileIdList = "";
            if (result.success) {
                if (str.length > 1) {
                    if (str[1] != "") {
                        var files = eval("(" + str[1] + ")");
                        ClientUploadFile(false,files);
                        if (upCallbackFunc != null) {
                            upCallbackFunc();

                            upCallbackFunc = null;
                        }
                        else {
                            window.location.reload();
                        }
                    }
                }
            }


        }
    });
}

var fileTableId;
var FileName, FileEXT, FileSize, FilePath, FileMathNum,PicPath;
function UploadFilesNew(o, isMultiply, tableId, callbackFunc, param1, param2, rejectTypes, override) {
    upCallbackFunc = callbackFunc;
    selectCallbackFunc = SelectFileCallbackNew;
    fileTableId = tableId;
    if ($(o).attr("fileObjId")) {
        $("#fileObjId").val($(o).attr("fileObjId"));
    }
    if ($(o).attr("fileTypeId")) {
        $("#fileTypeId").val($(o).attr("fileTypeId"));
    }
    if ($(o).attr("keyValue")) {
        $("#keyValue").val($(o).attr("keyValue"));
    }
    if ($(o).attr("tableName")) {
        $("#tableName").val($(o).attr("tableName"));
    }
    if ($(o).attr("keyName")) {
        $("#keyName").val($(o).attr("keyName"));
    }
    if ($(o).attr("uploadType")) {
        $("#uploadType").val($(o).attr("uploadType"));
    }
    if ($(o).attr("fileRel_profId")) {
        if (typeof $("#fileRel_profId").val() != "undefined") {
            $("#fileRel_profId").val($(o).attr("fileRel_profId"));
        }
    }
    if ($(o).attr("fileRel_stageId")) {
        if (typeof $("#fileRel_stageId").val() != "undefined") {
            $("#fileRel_stageId").val($(o).attr("fileRel_stageId"));
        }
    }
    if ($(o).attr("fileRel_fileCatId")) {
        if (typeof $("#fileRel_fileCatId").val() != "undefined") {
            $("#fileRel_fileCatId").val($(o).attr("fileRel_fileCatId"));
        }
    }
    if (isMultiply == "true") {
        if (rejectTypes != null && rejectTypes != undefined) {
            if (override == true) {
                SelectFile(false, true, rejectTypes);
            } else {
                SelectFile(false, true, mttRejectTypes + "," + rejectTypes);
            }
        } else {
            SelectFile(false, true, mttRejectTypes);
        }
        //console.log(12);
    }
    else if (isMultiply == "false") {
        if (rejectTypes != null && rejectTypes != undefined) {
            if (override == true) {
                SelectFile(false, false, rejectTypes);
            } else {
                SelectFile(false, false, mttRejectTypes + "," + rejectTypes);
            }
        } else {
            SelectFile(false, false, mttRejectTypes);
        }
    }
}

function SelectFileCallbackNew(fileList) {
    var propertyStr = "&";
    var fileObjId = $("#fileObjId").val();
    var fileTypeId = $("#fileTypeId").val();
    var keyValue = $("#keyValue").val();
    var tableName = $("#tableName").val();
    var keyName = $("#keyName").val();
    var fileRel_profId = $("#fileRel_profId").val();
    var fileRel_stageId = $("#fileRel_stageId").val();
    var fileRel_fileCatId = $("#fileRel_fileCatId").val();
    var uploadType = "";

    if ($("#uploadType")) {
        uploadType = $("#uploadType").val();
    }

    $("input[name^='Property_']").each(function (i, val) {
        var prop = $(this).attr("name") + "=" + $(this).attr("value") + "&";
        propertyStr += prop;
    });

    var d = fileList;
    var i = 0;
    var len = d.length;
    var str = "";
    var o;

    var NewUserFilePath = "";
    if ($("#uploadFileList")) {
        NewUserFilePath = $("#uploadFileList").val();
    }
    var splitStr = "|H|";

    for (; i < len; ++i) {
        o = d[i];
        FileMathNum = parseInt(Math.random() * 10000000);
        if (NewUserFilePath == "") {
            NewUserFilePath = o.NativePath;
        }
        else {
            NewUserFilePath += splitStr + o.NativePath;
        }
        NewUserFilePath += ("|N|" + FileMathNum + "|Y|" + o.RootDir)
        NewUserFilePath += ("|Y|fileObjId|-|" + fileObjId + "|N|fileTypeId|-|" + fileTypeId + "|N|uploadType|-|" + uploadType + "|N|keyValue|-|" + keyValue + "|N|keyName|-|" + keyName + "|N|tableName|-|" + tableName + "|N|isCover|-|No");
        FileName = d[i].Name;
        FileEXT = d[i].Type || '';
        FilePath = o.NativePath + "|N|" + FileMathNum + "|Y|" + o.RootDir + "|Y|fileObjId|-|" + fileObjId + "|N|fileTypeId|-|" + fileTypeId + "|N|uploadType|-|" + uploadType + "|N|keyValue|-|" + keyValue + "|N|keyName|-|" + keyName + "|N|tableName|-|" + tableName + "|N|isCover|-|No";
        FileName = FileName.replace(FileEXT, '');
        FileSize = d[i].FileSizeCalc;
        PicPath = o.NativePath;
        addFileTr();
    }
    if ($("#uploadFileList")) {
        $("#uploadFileList").val(NewUserFilePath);
    }
    fileHint(4, len);
    //增加回调函数调用，在点击确定上传时执行
    if (upCallbackFunc != null) {
        upCallbackFunc();

        upCallbackFunc = null;
    }
}

function addFileTr() {
    var html = "";
    html += '<tr><td align="center"><div class="tb_con"><input type="checkbox" fileName="fileInputNo" fileRelId="' + FilePath + '"/></div></td>'
    html += '<td><div class="tb_con"> <a href="javascript:void(0);" class="gray" onclick=\'alertRead()\'>';
    html += '<img src="/Content/images/icon/error.png" style="visibility: inherit" />'
    html += FileName;
    html += '</a></div></td><td align="center"><div class="tb_con">';
    html += FileEXT;
    html += '</div></td> <td align="center"><div class="tb_con">';
    html += FileSize;
    html += '</div></td><td align="center"><div class="tb_con">';

    if (fileTableId.indexOf("cover") != -1) {
        html += '<input type="radio" name="imgFirst" value="1" fileTableId="' + fileTableId + '" onclick=\'SetCoverImageNew("' + FileMathNum + '",this);\' />设置封面图 &nbsp;'
    } html += ' <a href="javascript:void(0);" class="gray" onclick=\'alertRead()\'>阅读</a> '
    html += '<a href="javascript:void(0);"class="gray" onclick=\'alertDown();\'> 下载</a>';
    html += ' <a href="javascript:void(0);" class="red" Num="' + FileMathNum + '" path="' + FilePath + '" onclick=\'delFileRow(this)\'>删除</a>';
    html += '</div></td></tr>';
    $("#" + fileTableId).find('tr:last').after(html);


}


function alertDown() {
    alert("请先保存后再下载!");
}
function alertRead() {
    alert("请先保存后再阅读!");
}
function delFileRow(obj) {
    $(obj).parent().parent().parent().remove();
    var tempPath = $(obj).attr("path");
    var tempUploadFileList = $("#uploadFileList").val();
    if (tempUploadFileList.indexOf(tempPath + "|H|") != -1) {
        $("#uploadFileList").val(tempUploadFileList.replace(tempPath + "|H|", ""));
    }
    else if (tempUploadFileList.indexOf(tempPath) != -1) {
        $("#uploadFileList").val(tempUploadFileList.replace(tempPath, ""));
    }
}
function SetCoverImageNew(id, obj) {
    var tempTableId = $(obj).attr("fileTableId");
    var tempPath = $(obj).parent().find("a[Num=" + id + "]").attr("path");
    if ($(obj).prop("checked") == true) {
        $(obj).parent().find("a[Num=" + id + "]").attr("path", tempPath.replace("isCover|-|No", "isCover|-|Yes"));
        $(obj).parent().parent().parent().find("input[fileName=fileInputNo]").attr("fileRelId", tempPath.replace("isCover|-|No", "isCover|-|Yes"));
        if ($("#uploadFileList").val().indexOf(tempPath) != -1) {
            $("#uploadFileList").val($("#uploadFileList").val().replace(tempPath, $(obj).parent().find("a[Num=" + id + "]").attr("path")));
        }
        $.tmsg("m_jfw", "设置成功！", { infotype: 1 });

    }
    else if ($(obj).prop("checked") == false) {
        $(obj).parent().find("a[Num=" + id + "]").attr("path", tempPath.replace("isCover|-|Yes", "isCover|-|No"));
        $(obj).parent().parent().parent().find("input[fileName=fileInputNo]").attr("fileRelId", tempPath.replace("isCover|-|Yes", "isCover|-|No"));
        if ($("#uploadFileList").val().indexOf(tempPath) != -1) {
            $("#uploadFileList").val($("#uploadFileList").val().replace(tempPath, $(obj).parent().find("a[Num=" + id + "]").attr("path")));
        }
        $.tmsg("m_jfw", "设置成功！", { infotype: 1 });
    }
}


//新增上传、删除文件提示  param1 : 参数（上传多少个文件，删除多少个文件）  optype 1：删除未上传的文件 2：删除已上传的文件 3 撤销删除 4 上传文件
function fileHint(optype, param1) {
    if (typeof $("#filehint").attr("filenum") != "undefined") {
        $("#filehint").html("");
        var fileNum = $("#filehint").attr("filenum");
        var uploadNum = $("#filehint").attr("uploadnum");
        var fileNum1 = parseInt(fileNum);
        var uploadNum1 = parseInt(uploadNum);
        if (optype == 1) {
            uploadNum1 = uploadNum1 - param1;
        } else if (optype == 2) {
            fileNum1 = fileNum1 + param1;
        }
        else if (optype == 3) {
            fileNum1 = fileNum1 - param1;
        } else if (optype == 4) {
            uploadNum1 = uploadNum1 + param1;
        }

        if (uploadNum1 != 0 && fileNum1 != 0) {
            $("#filehint").html("已选择：<span class='filenum red'>" + uploadNum1 + "</span>个文件上传、" + "<span class='filenum red'>" + fileNum1 + "</span>个文件删除，请点击保存！");
        }
        else if (uploadNum1 != 0 && fileNum1 == 0) {
            $("#filehint").html("已选择:<span class='filenum red'>" + uploadNum1 + "</span>个文件上传，请点击保存！！");
        }
        else if (uploadNum1 == 0 && fileNum1 != 0) {
            $("#filehint").html("已选择:<span class='filenum red'>" + fileNum1 + "</span>个文件删除，请点击保存！");
        }
        $("#filehint").attr("filenum", fileNum1);
        $("#filehint").attr("uploadnum", uploadNum1);
    }
}

//删除行 id：关联Id
function delTr(obj, id) {
    if (id == 0) {
        $(obj).parent().parent().parent().remove();
        var tempPath = $(obj).attr("path");
        var tempUploadFileList = $("#uploadFileList").val();
        if (tempUploadFileList.indexOf(tempPath + "|H|") != -1) {
            $("#uploadFileList").val(tempUploadFileList.replace(tempPath + "|H|", ""));
        }
        else if (tempUploadFileList.indexOf(tempPath) != -1) {
            $("#uploadFileList").val(tempUploadFileList.replace(tempPath, ""));
        }
        fileHint(1, 1);
    } else {
        var $parentObj = $(obj).parent();
        $parentObj.parent().addClass("preDel");
        $parentObj.find("a").last().remove();
        $parentObj.find("input").attr('disabled', 'disabled').removeAttr('checked');
        $parentObj.append("<a class='link removeDel' href='javascript:void(0);' onclick='cancelDelTr(" + id + ",this);'>撤销删除</a>");
        var fileRels = $("#delFileRelIds").val();
        if (fileRels == "") {
            $("#delFileRelIds").val(id);
        }
        else {
            $("#delFileRelIds").val(fileRels + "," + id);
        }
        fileHint(2, 1);
    }
}

//撤销删除（未实时删除） id：关联Id
function cancelDelTr(id, obj) {
    var fileRels = $("#delFileRelIds").val();
    fileRels = "," + fileRels + ",";
    if (fileRels.indexOf("," + id + ",") != -1) {
        fileRels = fileRels.replace("," + id + ",", ",");
    }
    fileRels = fileRels.substr(1, fileRels.length - 1);
    $("#delFileRelIds").val(fileRels);
    var $parentObj = $(obj).parent();
    $(obj).parent().parent().removeClass("preDel");
    $(obj).parent().html();
    $(obj).parent().find("a").last().remove();
    $parentObj.append("<a class='link' href='javascript:void(0);' onclick='delTr(this," + id + ");'>删除</a>");
    $parentObj.find("input").removeAttr('disabled');
    fileHint(3, 1);
}

//在已存在的文件夹下上传新文件
//上传类型默认为文件类型，暂不支持文件夹
var dataArr = [];
var struct;
function UploadFilesWithFilterAndStruct(o, isMultiply, structId, callbackFunc, rejectTypes, override) {
    upCallbackFunc = callbackFunc;
    dataArr = [];
    struct = structId;
    selectCallbackFunc = SelectStructFileCallback;
    if ($(o).attr("fileObjId")) {
        dataArr.push("fileObjId=" + $(o).attr("fileObjId"));
    }
    if ($(o).attr("fileTypeId")) {
        dataArr.push("&fileTypeId=" + $(o).attr("fileTypeId"));
    }
    if ($(o).attr("keyValue")) {
        dataArr.push("&keyValue=" + $(o).attr("keyValue"));
    }
    if ($(o).attr("tableName")) {
        dataArr.push("&tableName=" + $(o).attr("tableName"));
    }
    if ($(o).attr("keyName")) {
        dataArr.push("&keyName=" + $(o).attr("keyName"));
    }
    if ($(o).attr("fileRel_profId")) {
        if (typeof $("#fileRel_profId").val() != "undefined") {
            dataArr.push("&fileRel_profId=" + $(o).attr("fileRel_profId"));
        }
    }
    if ($(o).attr("fileRel_stageId")) {
        if (typeof $("#fileRel_stageId").val() != "undefined") {
            dataArr.push("&fileRel_stageId=" + $(o).attr("fileRel_stageId"));
        }
    }
    if ($(o).attr("fileRel_fileCatId")) {
        if (typeof $("#fileRel_fileCatId").val() != "undefined") {
            dataArr.push("&fileRel_fileCatId=" + $(o).attr("fileRel_fileCatId"));
        }
    }
    if (isMultiply == "true") {
        if (rejectTypes != null && rejectTypes != undefined) {
            if (override == true) {
                SelectFile(false, true, rejectTypes);
            } else {
                SelectFile(false, true, mttRejectTypes + "," + rejectTypes);
            }
        } else {
            SelectFile(false, true, mttRejectTypes);
        }
    }
    else if (isMultiply == "false") {
        if (rejectTypes != null && rejectTypes != undefined) {
            if (override == true) {
                SelectFile(false, false, rejectTypes);
            } else {
                SelectFile(false, false, mttRejectTypes + "," + rejectTypes);
            }
        } else {
            SelectFile(false, false, mttRejectTypes);
        }
    }
}
function SelectStructFileCallback(fileList) {
    var d = fileList;
    var i = 0;
    var len = d.length;
    var str = "";
    var o;

    var NewUserFilePath = "";

    var splitStr = "|H|";

    var propertyStr = "&";

    $("input[name^='Property_']").each(function (i, val) {
        var prop = $(this).attr("name") + "=" + $(this).attr("value") + "&";
        propertyStr += prop;
    });

    for (; i < len; ++i) {
        o = d[i];
        if (NewUserFilePath == "") {
            NewUserFilePath = o.NativePath;
        }
        else {
            NewUserFilePath += splitStr + o.NativePath;
        }
        NewUserFilePath += ("|Y|" + o.RootDir);

    }

    dataArr.push("&uploadType=2");
    dataArr.push("&uploadFileList=" + encodeURIComponent(NewUserFilePath));
    dataArr.push("&structId=" + struct);

    var dataStr = dataArr.join('');

  

    $("input[fileattributes=true]").each(function () {
        if ($(this).val() != null) {
            dataStr += "&" + $(this).attr("name") + "=" + $(this).val();
            $(this).val(null);
        }
    });
    var arr;
    $("body").maskInfo({ loadType: "10" });
    $.ajax({
        url: '/home/SaveMultipleUploadFiles',
        type: 'post',
        data: dataStr + propertyStr,
        dataType: 'html',
        error: function () {
            $("body").unmaskInfo();
            alert("添加失败");
        },
        success: function (data) {
            $("body").unmaskInfo();
            var str = data.split("|");
            var result = eval("(" + str[0] + ")");
            var fileIdList = "";
            if (result.success) {
                if (str.length > 1) {
                    if (str[1] != "") {
                        var files = eval("(" + str[1] + ")");
                        ClientUploadFile(false,files);
                        if (upCallbackFunc != null) {
                            upCallbackFunc();

                            upCallbackFunc = null;
                        }
                        else {
                            setTimeout("window.location.reload();", 50) 
                        }

                    }

                }
            }
        }
    });
}

//文件夹下载
function DownLoadPackFile(structId) {
    $("body").maskInfo({ loadType: "10" });
    $.ajax({
        url: "/FileLib/GetFileStructInfo",
        type: 'post',
        data: { structIds: structId },
        cache: false,
        dataType: 'html',
        error: function () {
            $("body").unmaskInfo();
            hiAlert('未知错误，请联系服务器管理员，或者刷新页面重试', '数据读取失败');
        },
        success: function (data) {
            $("body").unmaskInfo();
            if (data.Success == false) {
                hiAlert(data.msgError, '数据读取失败');
            }
            else {
                var RetStr = data.split("|")[1];
                if (RetStr != "") {
                    RetStr = eval('(' + RetStr + ')');
                    var downlist = [];
                    var filesCount = 0;
                    var upfilesCount = 0;
                    $(RetStr).each(function () {
                        if (this.guid != "") {
                            downlist.push({ guid: this.guid, name: this.name, ext: this.ext, size: this.size, path: this.filePath });
                            filesCount++;
                        }
                        else {
                            upfilesCount++;
                        }
                    });
                    if (downlist.length) {
                        ClientDownloadFiles(false, downlist);
                        $.tmsg("m_jfw", "下载文件,共" + filesCount + "个文件。");
                        if (upfilesCount != 0) {
                            $.tmsg("m_jfw", "共" + upfilesCount + "个文件，未上传成功，无法下载！");
                        }
                    }
                    else {
                        $.tmsg("m_jfw", "共" + upfilesCount + "个文件，未上传成功，无法下载！");
                    }
                } else {
                    $.tmsg("m_jfw", "没有文件需要下载。");
                }
            }
        }
    });

}

//文件文件夹重命名
function RenameFileOrStruct(tableName, id, name, relTable, filterJson, callBack) {
    $.ajax({
        url: "/FileLib/RenameFileOrStruct",
        type: 'post',
        data: {
            tableName: tableName,
            id: id,
            name: name,
            relTable: relTable,
            filterJson: filterJson
        },
        dataType: 'json',
        error: function () {
            alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
        },
        success: function (data) {
            if (data.Success == false) {
                alert(data.Message, '删除失败');
            }
            else {
                if (callBack != null) {
                    callBack();
                } else {
                    window.location.reload();
                }
            }
        }
    });
}

//文件夹版本更新
function UploadStructNewVersion(o,callbackFunc,rejectTypes,override) {
    upCallbackFunc = callbackFunc;
    selectCallbackFunc = SelectNewFolderCallBack;
    if ($(o).attr("fileObjId")) {
        $("#fileObjId").val($(o).attr("fileObjId"));
    }
    if ($(o).attr("fileTypeId")) {
        $("#fileTypeId").val($(o).attr("fileTypeId"));
    }
    if ($(o).attr("keyValue")) {
        $("#keyValue").val($(o).attr("keyValue"));
    }
    if ($(o).attr("tableName")) {
        $("#tableName").val($(o).attr("tableName"));
    }
    if ($(o).attr("keyName")) {
        $("#keyName").val($(o).attr("keyName"));
    }
    if ($(o).attr("uploadType")) {
        $("#uploadType").val($(o).attr("uploadType"));
    }
    if ($(o).attr("fileRel_profId")) {
        if (typeof $("#fileRel_profId").val() != "undefined") {
            $("#fileRel_profId").val($(o).attr("fileRel_profId"));
        }
    }
    if ($(o).attr("fileRel_stageId")) {
        if (typeof $("#fileRel_stageId").val() != "undefined") {
            $("#fileRel_stageId").val($(o).attr("fileRel_stageId"));
        }
    }
    if ($(o).attr("fileRel_fileCatId")) {
        if (typeof $("#fileRel_fileCatId").val() != "undefined") {
            $("#fileRel_fileCatId").val($(o).attr("fileRel_fileCatId"));
        }
    }
    if ($(o).attr("structId")) {
        if (typeof $("#structId").val() != "undefined") {
            $("#structId").val($(o).attr("structId"));
        }
    }
    if (rejectTypes != null && rejectTypes != undefined) {
        if (override == true) {
            SelectFile(false, true, rejectTypes);
        } else {
            SelectFile(false, true, mttRejectTypes + "," + rejectTypes);
        }
    } else {
        SelectFile(false, true, mttRejectTypes);
    }
}
function SelectNewFolderCallBack(fileList) {
    var propertyStr = "&";
    var fileObjId = $("#fileObjId").val();
    var fileTypeId = $("#fileTypeId").val();
    var keyValue = $("#keyValue").val();
    var tableName = $("#tableName").val();
    var keyName = $("#keyName").val();
    var fileRel_profId = $("#fileRel_profId").val();
    var fileRel_stageId = $("#fileRel_stageId").val();
    var fileRel_fileCatId = $("#fileRel_fileCatId").val();
    var structId = $("#structId").val();
    var uploadType = "2";

    $("input[name^='Property_']").each(function (i, val) {
        var prop = $(this).attr("name") + "=" + $(this).attr("value") + "&";
        propertyStr += prop;
    });

    var d = fileList
    var i = 0;
    var len = d.length;
    var str = "";
    var o;

    var NewUserFilePath = "";

    var splitStr = "|H|";

    for (; i < len; ++i) {
        o = d[i];
        if (NewUserFilePath == "") {
            NewUserFilePath = o.NativePath;
        }
        else {
            NewUserFilePath += splitStr + o.NativePath;
        }
        NewUserFilePath += ("|Y|" + o.RootDir);

    }
    var dataStr = "fileObjId=" + fileObjId;
    dataStr += "&fileTypeId=" + fileTypeId;
    dataStr += "&uploadFileList=" + encodeURIComponent(NewUserFilePath);
    dataStr += "&tableName=" + tableName;
    dataStr += "&keyName=" + keyName;
    dataStr += "&keyValue=" + keyValue;
    dataStr += "&uploadType=" + uploadType;
    dataStr += "&fileRel_profId=" + fileRel_profId;
    dataStr += "&fileRel_stageId=" + fileRel_stageId;
    dataStr += "&fileRel_fileCatId=" + fileRel_fileCatId;
    dataStr += "&structId=" + structId;
    $("input[fileattributes=true]").each(function () {
        if ($(this).val() != null) {
            dataStr += "&" + $(this).attr("name") + "=" + $(this).val();
            $(this).val(null);
        }
    });
    var arr;
    $("body").maskInfo({ loadType: "10" });
    $.ajax({
        url: '/FileLib/SaveNewStructVersion',
        type: 'post',
        data: dataStr + propertyStr,
        dataType: 'html',
        error: function () {
            $("body").unmaskInfo();
            alert("添加失败");
        },
        success: function (data) {
            $("body").unmaskInfo();
            var str = data.split("|");
            var result = eval("(" + str[0] + ")");
            var fileIdList = "";
            if (result.success) {
                if (str.length > 1) {
                    if (str[1] != "") {
                        var files = eval("(" + str[1] + ")");
                        ClientUploadFile(false,files);

                        if (upCallbackFunc != null) {
                            upCallbackFunc();
                            upCallbackFunc = null;
                        }
                        else {
                            window.location.reload();
                        }
                    }
                }
            }
        }
    });
}

function FileViewCount(id, ver) {
    $.ajax({
        url: "/Home/FileViewCount",
        type: 'post',
        data: 'fileId=' + id + "&version=" + ver,
        dataType: 'json',
        error: function () {
            alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
        },
        success: function (data) {
        }
    })
}
function FileDownloadCount(ids){
    $.ajax({
        url: "/Home/FileDownloadCount",
        type: 'post',
        data: 'fileIds=' + ids,
        dataType: 'json',
        error: function () {
            alert('未知错误，请联系服务器管理员，或者刷新页面重试', '保存失败');
        },
        success: function (data) {
        }
    })
}

function PreFileDelete(projId,tbName, id) {
    var data = "projId=" + projId;
    if (tbName == "FileLibrary") {
        data += "&fileIds=" + id;
    } else {
        data += "&structIds=" + id;
    }
    $.ajax({
        url: "/FileLib/PushDelete",
        type: 'post',
        data: data,
        dataType: 'json',
        error: function () {
            alert("未知错误,请联系服务器管理员，或者刷新页面重试!");
        },
        success: function (data) {
            if (data.Success == false) {
                alert("推送失败 : " + data.Message);
            }
            else {
                $.tmsg("m_jfw", "推送成功!", { infotype: 1, time_out: 1000 });
            }
        }
    });
}

function BatchPreFileDelete(projId,tbid) {
    var $selectfiles = $("#" + tbid + ",#j-taskdocfileDiv").find("input[name=fileList]:checked");
    var $selectStructs = $('#' + tbid + ",#j-taskdocfileDiv").find("input[name=fileStructList]:checked");
    var fileIdArr = [], fileStructIdArr = [];
    $.each($selectfiles, function (index, item) {
        if ($(item).attr("candel") == '1') {//判断是否可以删除
            fileIdArr.push($(item).attr("frelid"));
        }
    });
    $.each($selectStructs, function (index, item) {
        if ($(item).attr("candel") == '1') {//判断是否可以删除
            fileStructIdArr.push($(item).attr("structId"));
        }
    });

    if ((fileIdArr.length + fileStructIdArr.length) <= 0) {
        alert('请选择要推送删除的文件或文件夹');
        return false;
    }

    if (!confirm("确定要删除所选的文件或文件夹给管理员删除吗？")) {
        return false;
    }
    var data ="projId=" + projId + "&fileIds=" + fileIdArr.join(',') + "&structIds=" + fileStructIdArr.join(',');
    $.ajax({
        url: "/FileLib/PushDelete",
        type: 'post',
        data: data,
        dataType: 'json',
        error: function () {
            alert("未知错误,请联系服务器管理员，或者刷新页面重试!");
        },
        success: function (data) {
            if (data.Success == false) {
                alert("推送失败 : " + data.Message);
            }
            else {
                $.tmsg("m_jfw", "推送成功!", { infotype: 1 ,time_out: 1000});
            }
        }
    });
}

function cancelPushDelete(tb, id) {
    $.ajax({
        url: "/FileLib/CancelPushDelete",
        type: 'post',
        data: {
            tableName: tb,
            id: id
        },
        dataType: 'json',
        error: function () {
            alert("未知错误,请联系服务器管理员，或者刷新页面重试!");
        },
        success: function (data) {
            if (data.Success == false) {
                alert("删除失败 : " + data.Message);
            }
            else {
                location.reload();
            }
        }
    });
}
