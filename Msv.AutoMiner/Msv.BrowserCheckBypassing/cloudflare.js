/*
Source script sample (2017-12-11):

(function(){
var a = function() {try{return !!window.addEventListener} catch(e) {return !1} },
b = function(b, c) {a() ? document.addEventListener("DOMContentLoaded", b, c) : document.attachEvent("onreadystatechange", b)};
b(function(){
  var a = document.getElementById('cf-content');a.style.display = 'block';
  setTimeout(function(){
	var s,t,o,p,b,r,e,a,k,i,n,g,f, gOqdGVl={"sdMbF":+((!+[]+!![]+!![]+!![]+!![]+[])+(+[]))};
	t = document.createElement('div');
	t.innerHTML="<a href='/'>x</a>";
	t = t.firstChild.href;r = t.match(/https?:\/\//)[0];
	t = t.substr(r.length); t = t.substr(0,t.length-1);
	a = document.getElementById('jschl-answer');
	f = document.getElementById('challenge-form');
	;gOqdGVl.sdMbF+=+((+!![]+[])+(!+[]+!![]+!![]+!![]));gOqdGVl.sdMbF*=+((!+[]+!![]+[])+(!+[]+!![]));gOqdGVl.sdMbF*=+((!+[]+!![]+!![]+!![]+[])+(!+[]+!![]+!![]+!![]+!![]+!![]+!![]+!![]+!![]));gOqdGVl.sdMbF-=+((!+[]+!![]+!![]+!![]+[])+(!+[]+!![]+!![]+!![]));gOqdGVl.sdMbF+=+((!+[]+!![]+[])+(!+[]+!![]));gOqdGVl.sdMbF-=+((!+[]+!![]+!![]+!![]+[])+(!+[]+!![]+!![]+!![]));gOqdGVl.sdMbF-=+((!+[]+!![]+!![]+[])+(!+[]+!![]+!![]+!![]));gOqdGVl.sdMbF+=+((!+[]+!![]+[])+(!+[]+!![]));gOqdGVl.sdMbF+=+((!+[]+!![]+!![]+[])+(!+[]+!![]+!![]+!![]+!![]+!![]+!![]+!![]));a.value = parseInt(gOqdGVl.sdMbF, 10) + t.length; '; 121'
	f.action += location.hash;
	f.submit();
  }, 4000);
}, false);
})();

Source challenge form:
  <form id="challenge-form" action="/cdn-cgi/l/chk_jschl" method="get">
    <input type="hidden" name="jschl_vc" value="f8ffdea90d39bcfbef7b740e69047063"/>
    <input type="hidden" name="pass" value="1512981553.006-wcyvy4FX9B"/>
    <input type="hidden" id="jschl-answer" name="jschl_answer"/>
  </form>

*/

// DOM members
var location = { hash: "" };

var window = {
    addEventListener: true
}

var document = {
    addEventListener: function(event, handler) {
        handler();
    },
    getElementById: function(id) {
        if (/content$/.test(id))
            return { style: { display: null } };
        switch (id) {
        case "jschl-answer":
            return resultField;
        case "challenge-form":
            return {
                action: "",
                submit: function() {}
            };
        default:
            return null;
        }
    },
    createElement: function(tag) {
        if (tag === "div")
            return createdDiv;
        return {};
    }
}

function setTimeout(func, msec) {
    timeout = msec;
    func();
}

// Script members
var timeout = 0;
var createdDiv = {
    innerHTML: "",
    firstChild: { href: "" }
};
var resultField = { value: null };

function setSourceUri(uri) {
    createdDiv.firstChild.href = uri;
}

function getAnswer() {
    return resultField.value;
}