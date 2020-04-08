# .NET HttpClient logging to HTTP Archive files

**WARNING: This is not a production quality code!**

Often it's beneficial to log application HTTP requests for later analysis or support. This small code sample shows how to easily insert a logger into applications using the .NET `HttpClient` class. The logger creates a minimalistic HAR file that is loadable in tools like Fiddler.

Limitations:
* Request timings are not reported correctly
* Header and content sizes are not reported correctly
* Cookies are not written to the output
* Other things marked with `FIXME` in the sample code
