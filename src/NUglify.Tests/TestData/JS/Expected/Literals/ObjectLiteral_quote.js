﻿function resetTop(elem){var $elem=$(elem),prev={"marginTop":$elem.css("margin-top")};return $elem.css({"marginTop":"auto"}),prev}var o={get ack(){return 42},set ack(v){alert(v)},"123":4560,"789.2":!0,"help":"me","while":45.67,"foo":function(){return"bar"},"goto":"what?","你好":"hello"},prop,es6;for(alert(o.goto);0;);({"foo":42,"showMe":function(){document.write("<h1>"+this.foo+"!<\/h1>")}}).showMe();prop="bar";es6={"myProperty":42,get foo(){return this.foo},set foo(value){this.foo=value},[prop]:"hey",["b"+"az"]:"there",["c"+x()]:"hello",[("ab"+"cd"+"ef").substring(2,4)]:"hi",location,myMethod(one,two){alert(one+two)},*myGenerator(array){for(var item of array)yield item}}