const peer = new Peer({
    key: _MYAPIKEY_,
    debug: 0,
});
const DataConnection = {};

peer.on('open', id => {
    console.log(id);
    var PID = document.getElementById('MyPeerID');
    PID.innerHTML = 'Peer ID : ' + id;
});

var WSC = new WebSocket('ws://localhost:12835');
WSC.close();
function WSConnecter() {
    WSC = new WebSocket("ws://localhost:12835");
    WSC.binaryType = 'arraybuffer';
    WSC.onopen = function () {
        console.log('WebSocket Open');
        var IsCDTexto = document.getElementById('IsSocketConnected');
        IsCDTexto.innerHTML = 'WebSocket 接続済み';
    };
    WSC.onclose = function () {
        console.log('WebSocket Close');
        var IsCDTextc = document.getElementById('IsSocketConnected');
        IsCDTextc.innerHTML = 'WebSocket 未接続';
    }
    WSC.onmessage = function (e) {
        var MSData = new Uint8Array(e.data.slice(0, 64));
        for (var cn in peer.connections) {
            if (peer.connections[cn][0].open) {
                console.log("SkyWay WSDataSender : ", peer.connections[cn][0]);
                peer.connections[cn][0].send(MSData);
            }
        }
        console.log('WebSocket DataGet : ' , MSData);
    };
    WSC.onerror = function (error) {
        console.error('WebSocket エラー : ', error);
    };

}

window.onunload = function () {
    peer.destroy();
    WSC.close();
};

peer.on('connection', c => {
    console.log(c);
    //console.log(peer.connections);
    c.on('open',()=>{
        DataConnection[c.remoteID] = c;
        console.log('SkyWay DataOpen : ', c);
    });
    c.on('close',()=>{
        console.log('SkyWay DataClose : ', c);
        delete DataConnection[c.remoteID];
    });
    c.on('data',function(data){
        console.log('SkyWay DataGet : ', data);
        WSC.send(data);
    });
  });

  function ReConnect(){
    console.log('WebSocket 再接続');
      WSC.close();
      WSConnecter();
}




  