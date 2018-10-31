/*! Copyright (c) 2012 Radar
* Licensed under the BSD License (LICENSE.txt).
*
* Version 1
*/

//Json to string
function JTS(o) {
    var arr = [];
    var fmt = function (s) {
        if (typeof s == 'object' && s != null) return JsonToStr(s);
        return /^(string|number)$/.test(typeof s) ? "'" + s + "'" : s;
    }
    for (var i in o)
        arr.push("'" + i + "':" + fmt(o[i]));
    return '{' + arr.join(',') + '}';
}

//控制输入框只能输入数字和小数点
function InitCheckNum() {
    $(".checkNum").keypress(function (event) {
        var keyCode = event.which;
        if (keyCode == 46 || (keyCode >= 48 && keyCode <= 57) || keyCode == 8)//8是删除键  
            return true;
        else
            return false;
    }).focus(function () {
        this.style.imeMode = 'disabled';
    });
}


function InitMoney() {
    $(".money").keypress(function (event) {
        var keyCode = event.which;
        if (keyCode == 46 || (keyCode >= 48 && keyCode <= 57) || keyCode == 8)//8是删除键  
        {
            return true;
        }
        else {
            return false;
        }
    }).focus(function () {
        this.style.imeMode = 'disabled';
    });

    $(".money").focus(function () {
        var val = $(this).val();
        if (val != "") {
            val = numeral().unformat(val);
            //if (val == 0) val = "";
            $(this).val(val);
        }
        var r = $(this)[0].createTextRange(); //将光标定位在最后
        r.select();
        r.collapse(false);
    });

    $(".money").blur(function () {
        var val = $(this).val();
        if (val != "") {
            val = moneyFormat(val);
        } else {
            val = "";
        }
        $(this).val(val);
    });
}

function InitAreaNum() {
    $(".areaNum").keypress(function (event) {
        var keyCode = event.which;
        if (keyCode == 46 || (keyCode >= 48 && keyCode <= 57) || keyCode == 8)//8是删除键  
        {
            return true;
        }
        else {
            return false;
        }
    }).focus(function () {
        this.style.imeMode = 'disabled';
    });

    $(".areaNum").focus(function () {
        var val = $(this).val();
        if (val != "") {
            val = numeral().unformat(val);
            if (val == 0) val = "";
            $(this).val(val);
        }
        var r = $(this)[0].createTextRange(); //将光标定位在最后
        r.collapse(false);
        r.select();
    });

    $(".areaNum").blur(function () {
        var val = $(this).val();
        if (val != 0) {
            val = areaFormat(val);
        } else {
            val = "";
        }
        $(this).val(val);
    });
}

function areaFormat(val) {
    return numeral(val).format('0,0.00');
}

function moneyFormat(val) {
    return numeral(val).format('0,0.00');
}


function show_todoList() {
    var width = $("#todoList").width();
    if (width == 24) {
        $("#todoList").width(326);
    } else {
        $("#todoList").width(24);
    }
}



//通用删除方法(对象)
function CustomDeleteItem(obj, func, parameter, parameter1) {
    var a = confirm("确定要删除该对象吗？删除后将无法恢复");
    if (!a) return;
    var tbName = ""
    if ($(obj).attr("querystr")) {
        tbName = $(obj).attr("tbname")
    }
    var queryStr = ""
    if ($(obj).attr("querystr")) {
        queryStr = $(obj).attr("querystr");
    }
    $.ajax({
        url: "/Home/DelePostInfo/",
        type: 'post',
        data: { tbName: tbName, queryStr: queryStr },
        dataType: 'json',
        error: function () {
            layer.msg("未知错误，请联系服务器管理员，或者刷新页面重试", { icon: 5 });
            //$.tmsg("m_jfw", "未知错误，请联系服务器管理员，或者刷新页面重试", { infotype: 2 });
        },
        success: function (data) {
            if (data.Success == false) {
                layer.msg(data.Message, { icon: 5 });
                //alert(data.Message);
            }
            else {
                layer.msg("删除成功", { icon: 6 });
                // $.tmsg("m_jfw", "删除成功！", { infotype: 1 });
                if (func) { func(parameter, parameter1); } else { window.location.reload(); }
            }
        }
    });
}

//通过主键删除对应信息
function CommonDelByPK(tbName, keyVal, relTbName,relKey,func) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复!")) {
        var postData = { tbName: tbName, keyVal: keyVal, relTbName: relTbName, relKey: relKey };

        $.post("/Home/CommonDelByPK", postData, function (data) {
            
            if (data.Success) {
                if (func) {
                    func();
                } else {
                    window.location.reload();
                }
            } else {
                $.tmsg("m_jfw", data.msg, { infotype: 2 });
            }
        });
    }
}

//删除数据：relInfos格式为："relTbName1,relKey1|relTbName2,relKey2"
function CommonDel(tbName, keyName, keyValue, relInfos,func) {
    if (confirm("确定要删除该对象吗？删除后将无法恢复!")) {
        $.post("/Home/CommonDel", { tbName: tbName, keyName: keyName, keyValue: keyValue, relInfos: relInfos, func: func }, function (data) {

            if (data.Success) {
                if (func) {
                    func(data);
                } else {
                    $.tmsg("m_jfw", "数据删除成功，页面将刷新！", { infotype: 1 });
                    window.setTimeout("window.location.reload()", 1000);

                }
            } else {
                $.tmsg("m_jfw", data.msg, { infotype: 2 });
            }
        });
    }
}



//通用删除方法（参数）
function CustomDeleteItemStr(tbName, queryStr, func, parameter) {
    var a = confirm("确定要删除该对象吗？删除后将无法恢复");
    if (!a) return;
    $.ajax({
        url: "/Home/DelePostInfo/",
        type: 'post',
        data: { tbName: tbName, queryStr: queryStr },
        dataType: 'json',
        error: function () {
            $.tmsg("m_jfw", "未知错误，请联系服务器管理员，或者刷新页面重试", { infotype: 2 });
        },
        success: function (data) {
            if (data.Success == false) {
                alert(data.Message);
            }
            else {
                $.tmsg("m_jfw", "删除成功！", { infotype: 1 });
                if (func) { func(parameter); } else { window.location.reload(); }
            }
        }
    });
}

//通用批量删除
function BatchCustomDeleteItem(tbName, idClassName, idName, func, parameter) {
    var ids = ""
    $("." + idClassName).each(function () {
        if ($(this).find("input[type=checkbox]").attr("checked") == "checked" || $(this).find("input[type=checkbox]").attr("checked") == true) {  //yh2013-9-11: true替换为"checked"
            if (idName) {
                ids += $(this).attr("id").replace(idName, "") + ",";
            } else {
                ids += $(this).attr("id") + ",";
            }
        }
    });
    if (ids == "") {
        alert("请先选择要删除的对象！");
        return;
    }
    var a = confirm("确定要删除该对象吗？删除后将无法恢复");
    if (!a) return;
    $.ajax({
        url: "/Home/BatchDeleteByPrimaryKey/",
        type: 'post',
        data: { tbName: tbName, ids: ids },
        dataType: 'json',
        error: function () {
            $.tmsg("m_jfw", "未知错误，请联系服务器管理员，或者刷新页面重试", { infotype: 2 });
        },
        success: function (data) {
            if (data.Success == false) {
                alert(data.Message);
            }
            else {
                $.tmsg("m_jfw", "删除成功！", { infotype: 1 });
                if (func) { func(parameter); } else { window.location.reload(); }
            }
        }
    });
}

//基类批量移动
function MoveItemList(tbName, idClassName, idName, func, parameter) {
    var a = confirm("确定要批量移动吗？");
    if (!a) return;
    var ids = ""
    $("." + idClassName).each(function () {
        if ($(this).find("input[type=checkbox]").attr("checked") == true) {
            if (idName) {
                ids += $(this).attr("id").replace(idName, "") + ",";
            } else {
                ids += $(this).attr("id") + ",";
            }
        }
    });
    $.ajax({
        url: "/Home/BatchDeleteByPrimaryKey/",
        type: 'post',
        data: { tbName: tbName, ids: ids },
        dataType: 'json',
        error: function () {
            $.tmsg("m_jfw", "未知错误，请联系服务器管理员，或者刷新页面重试", { infotype: 2 });
        },
        success: function (data) {
            if (data.Success == false) {
                alert(data.Message);
            }
            else {
                $.tmsg("m_jfw", "移动成功！", { infotype: 1 });
                if (func) { func(parameter); } else { window.location.reload(); }
            }
        }
    });
}

///通用编辑修改
function CustomEditBox(url,_title,func) {
    var title =  _title;
    box(url, { boxid: 'methodEdit', title: title, width: 350, contentType: 'ajax',
        submit_cb: function (o) {
            var obj = o.fbox.find("form");
            CustomSaveForm(obj, func);
        }
    });
}
 



//通用保存方法
function CustomSaveForm(formId, func) {
    var formdata;
    if (typeof (formId) == "object") {
        formdata = formId.serialize();
    } else {
        formdata = $("#" + formId).serialize();
    }
    $.ajax({
        url: "/Home/SavePostInfo",
        type: 'post',
        data: formdata,
        dataType: 'json',
        error: function () {
            $.tmsg("m_jfw", "未知错误，请联系服务器管理员，或者刷新页面重试", { infotype: 2 });
        },
        success: function (data) {
            if (data.Success == false) {
                alert(data.Message);
            }
            else {
                var str = data.FileInfo.split("|");
                var result = eval("(" + str[0] + ")");
                if (result.success == true) {
                    var uuid = uploadUUID;
                    var fileIdList = "";
                    if (str.length > 1) {
                        if (str[1] != "") {
                            var files = eval("(" + str[1] + ")");
                            cwf.tasks.send(uuid, files, function () {
                                if (func) {
                                    func(data);
                                } else {
                                    alert("保存成功");
                                    window.location.reload();
                                }
                            });

                        }
                    }
                } else {
                    if (func) {  func(data); } else {
                        alert("保存成功");
                        window.location.reload();
                    }
                }
            }
        }
    });
}

//批量保存数据
function BatchSaveData(tbName, dataSource, func) {
    //数据格式：主键id_#主键id值|#字段名_#字段值|#字段名_#字段值*#目录id_#目录id值|#字段名_#字段值|#字段名_#字段值
    $.ajax({
        url: "/Home/BatchSaveDir?tbName=" + tbName,
        type: 'post',
        data: "dataSource=" + dataSource,
        dataType: 'json',
        error: function () {
            $.tmsg("m_jfw", "未知错误，请联系服务器管理员，或者刷新页面重试", { infotype: 2 });
        },
        success: function (data) {
            if (data.Success == false) {
                alert(data.Message);
            }
            else {
                if (func) { func(data); } else {
                    alert("保存成功");
                    window.location.reload();
                }
            }
        }
    });
}


var linkUrl = "";

function GotoResultViewPage(retId, typeId) {
    switch (typeId) {   //户型
        case "1":
            linkUrl = "/StandardResult/UnitView/"; break;
        //住宅标准层       
        case "2":
            linkUrl = "/StandardResult/ResidentialStandardView/"; break;
        //别墅户型        
        case "3":
            linkUrl = "/StandardResult/VillaUnitView/"; break;
        //别墅单元       
        case "4":
            linkUrl = "/StandardResult/VillaAtomView/"; break;
        //房型       
        case "5":
            linkUrl = "/StandardResult/UnitView/"; break;
        //室内精装修       
        case "6":
            linkUrl = "/StandardResult/FineDecorationView/"; break;
        //立面        
        case "7":
            linkUrl = "/StandardResult/FacadeView/"; break;
        //示范区       
        case "8":
            linkUrl = "/StandardResult/DemonstrationAreaView/"; break;
        //景观       
        case "9":
            linkUrl = "/StandardResult/LandscapeView/"; break;
        //标准工艺工法       
        case "10":
            linkUrl = "/StandardResult/PEMView/"; break;
        //公共部位       
        case "11":
            linkUrl = "/StandardResult/PublicPartsView/"; break;
        //设备与技术       
        case "12":
            linkUrl = "/StandardResult/EquipmentView/"; break;
        //绿化景观       
        case "13":
            linkUrl = "/StandardResult/GreenLandscapeView/"; break;
    }
    linkUrl += "?retId=" + retId + "&r=" + Math.random();
    return linkUrl;
}

function GotoResultEditPage(retId, typeId, catId, libId) {
    switch (typeId) {
        case "1":   //户型
            linkUrl = "/StandardResult/UnitEdit/"; break;
        case "2":   //住宅标准层  
            linkUrl = "/StandardResult/ResidentialStandardEdit/"; break;
        case "3":   //别墅户型  
            linkUrl = "/StandardResult/VillaUnitEdit/"; break;
        case "4":   //别墅单元
            linkUrl = "/StandardResult/VillaAtomEdit/"; break;
        case "5":   //房型  
            linkUrl = "/StandardResult/UnitEdit/"; break;
        case "6":   //室内精装修  
            linkUrl = "/StandardResult/FineDecorationEdit/"; break;
        case "7":   //立面  
            linkUrl = "/StandardResult/FacadeEdit/"; break;
        case "8":   //示范区 
            linkUrl = "/StandardResult/DemonstrationAreaEdit/"; break;
        case "9":   //景观 
        case "13":  //老年人活动场地 
        case "14":  //儿童活动场地
        case "15":  //运动场地
        case "16":  //水景
        case "17":  //小区出入口
        case "18":  //单元入户空间
        case "19":  //沿街商业空间
        case "20":  //地面停车场
        case "21":  //道路及广场
        case "22":  //地下车库出入口
            linkUrl = "/StandardResult/LandscapeEdit/"; break;
        case "10":  //标准工艺工法
            linkUrl = "/StandardResult/PEMEdit/";
            break;
        case "11":  //公共部位  
            linkUrl = "/StandardResult/PublicPartsEdit/"; break;
        case "12":  //设备与技术
        case "23":  //给水系统
        case "24":  //太阳能热水
        case "25":  //排水系统
        case "26":  //雨水系统
        case "27":  //消防水系统
        case "28":  //通风
        case "29":  //防排烟
        case "30":  //采暖系统
        case "31":  //空调系统
        case "32":  //燃气
        case "33":  //电器动力系统
        case "34":  //照明插座系统
        case "35":  //泛光照明
        case "36":  //防雷接地
        case "37":  //火灾自动报警系统
        case "38":  //视频监控系统
        case "39":  //周界报警系统
        case "40":  //电子巡查系统
        case "41":  //可视对讲系统
        case "42":  //室内报警系统
        case "43":  //一卡通及门禁系统
        case "44":  //电话宽带网络系统
        case "45":  //有线电视系统  
        case "46":  //停车库管理系统    
        case "47":  //信息发布系统
        case "48":  //智能家居系统
        case "49":  //远传抄袭系统
        case "50":  //公共及紧急广播系统
        case "51":  //其他
            linkUrl = "/StandardResult/EquipmentEdit/"; break;
        default:
            linkUrl = "/StandardResult/UnitEdit/";
            break;
    }
    linkUrl += "?catId=" + catId + "&retId=" + retId + "&typeId=" + typeId + "&libId=" + libId + "&r=" + Math.random();
    return linkUrl;
}

function ChangeUserScore(type,cbFunc) {
    $.ajax({
        url: "/Home/ChangeUserScore?type=" + type,
        type: 'post',
        data: {}, //data为空的也最好写上
        dataType: 'json',
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            if (console && console.log) {
                console.log("XMLHttpRequest.status:" + XMLHttpRequest.status);
                console.log("XMLHttpRequest.readyState:" + XMLHttpRequest.readyState);
                console.log("textStatus:" + textStatus);
                console.log("errorThrown:" + errorThrown);
            }
            $.tmsg("m_jfw", "未知错误，请联系服务器管理员，或者刷新页面重试", { infotype: 2 });
        },
        success: function (data) {
            if (data.Success == false) {
                alert(data.Message);
            } else if (typeof eval(cbFunc) === "function") {
                cbFunc();
            }
        }
    });
}
function ViewCount(tb, keyName, keyValue) {
    $.ajax({
        url: "/Home/ViewCount?tb=" + tb + "&keyName=" + keyName + "&keyValue=" + keyValue,
        type: 'post',
        dataType: 'json',
        error: function () {
            $.tmsg("m_jfw", "未知错误，请联系服务器管理员，或者刷新页面重试", { infotype: 2 });
        },
        success: function (data) {
            if (data.Success == false) {
                alert(data.Message);
            }
        }
    });
}


function changePassword() {
    var url = "/Account/ChangePassword";
    box(url, { boxid: "_changepassword", contentType: "ajax", width: 250,
        submit_cb: function (o) {
            var form = o.fbox.find("form");
            var newPassword = form.find("#newPassword").val();
            var verify = form.find("#verify").val();

            if (newPassword != verify) {
                alert("输入的两次新密码不一样！");
                return false;
            }

            var formdata = form.serialize();
            $.ajax({
                type: "post",
                url: url,
                data: formdata,
                async: false,
                success: function (data) {
                    if (data.success) {
                        $.tmsg("m_jfw", data.msg, { infotype: 1 });
                    } else {
                        $.tmsg("m_jfw", data.msg, { infotype: 2 });
                    }
                }
            });
           
        }
    });
}
