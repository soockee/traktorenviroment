# Traktor Test Enviroment

## Usage

Run the Project via:

```
docker-compose up
``` 


## Example

* Start the Enviroment
* traktor-fibonacci-server connects to the TraktorRegistry via WebSocket
* Send HTTP JSON to traktor-fibonacci-server with 1 Key:Value pair consisting of a Key named 'N' and a number e.g. with httpie
```
http PUT 172.22.0.4:8084 N:=2
``` 
* Result is the calculated Fibonacci-Number
* TraktorAgent recieves the SPANS and prints them


## ToDO
* Another Webserver which does the HTTP PUT for us, in order to thes tracer.inject and tracer.extract, which are responsible for ContextPropagation via TraktorRegistry