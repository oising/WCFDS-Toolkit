WCFDS-Toolkit
=============

WCF Data Services Toolkit NG

The original toolkit was discussed here on the original author's blog. The author did not
respond to several attempts to make contact, so I forked the project. The toolkit only supported
OData v2, which meant it did not support complex types, specifically collections thereof (a v3+ feature.)
I added support for this, fixed many outstanding bugs and upgraded the oDataLib to use the latest and
greatest (currently 5.6.0).

I then added some more supported "magic methods" for repository types to support complex types:

* CreateDefaultComplexType  (factory for complex types without a default constructor)

Collections of Complex Types as properties on entities are best expressed as IEnumerable of T, where T is
the type of the Complex Type. 

Here are the original blog posts which will give you a good introduction to the power of this library. For the
most part, I think ASP.NET WebApi has been pushed as the replacement for this, but again, that has too much
dependency on the entity framework in my opinion. This library will give you the same, but you can tie anything
at all into the back end.

Have fun.

http://lostintangent.com/post/3189655590/you-want-to-wrap-odata-around-what

http://lostintangent.com/post/4092419024/handling-updates-with-the-wcf-data-services-toolkit

https://wcfdstoolkit.codeplex.com/

