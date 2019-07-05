const app = {};

app.initCam = function() {
  // Grab elements, create settings, etc.
  var video = document.getElementById("cam");

  // Get access to the camera!
  if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
    // Not adding `{ audio: true }` since we only want video now
    navigator.mediaDevices.getUserMedia({ video: true }).then(function(stream) {
      video.srcObject = stream;
      video.play();
      app.startImageProcess();
    });
  }
};

// Start capturing images from camera
app.startImageProcess = function() {
  if (app.intervalId) {
    clearInterval(app.intervalId);
  }
  //app.intervalId = setInterval(app.captureImageFromCam.bind(app), 20000);
  app.captureImageFromCam();
};

app.captureImageFromCam = function() {
  // Elements for taking the snapshot
  var canvas = document.createElement("canvas"); //document.getElementById('cvs');
  canvas.height = 200;
  canvas.width = 200;
  var ctx = canvas.getContext("2d");
  var video = document.getElementById("cam");
  var imgContainer = document.getElementById("images");
  imgContainer.appendChild(canvas);
  ctx.drawImage(video, 0, 0, canvas.height, canvas.height);
  canvas.toBlob(app.uploadImage.bind(app), "image/jpg");
};

app.uploadImage = function(blob) {
  var list = document.getElementById("response-list");

  fetch("https://eastus.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceAttributes=emotion", {
    method: "POST",
    mode: "cors",
    headers: {
      "Content-Type": "application/octet-stream",
      "Ocp-Apim-Subscription-Key": "7042dce1bc9b45b0a57ebf58bb62cad9"
    },
    body: blob
  })
    .then(response => response.json())
    .then(json => {
      app.playAvatar(json);

      var li = document.createElement("li");
      li.innerHTML = JSON.stringify(json);
      list.appendChild(li);
      
      // scroll to end of container
      var parent = list.parentElement;
      parent.scrollTop = parent.scrollHeight;
      setTimeout(app.captureImageFromCam, 5000);
    });
};

app.playAvatar = function(json) {
  if(json.length > 0) {
    var response = json[0];
    var faceAttributes = response.faceAttributes.emotion;
    console.log(faceAttributes);
    if(faceAttributes.anger > 0.5) {
      app.WebAvatar.message("You are very angry");
    } else if(faceAttributes.anger > 0.1 && faceAttributes.anger < 0.5) {
      app.WebAvatar.message("You seems little angry");
    } else if(faceAttributes.happiness > 0.5) {
      app.WebAvatar.message("You are very happy today");
    } else if(faceAttributes.happiness > 0.1 && faceAttributes.happiness < 0.5) {
      app.WebAvatar.message("You seems happy today");
    } else if(faceAttributes.neutral > 0.5 ) {
      app.WebAvatar.message("You looks neutral");
    } else if(faceAttributes.sadness > 0.5) {
      app.WebAvatar.message("Why are you so sad today?");
    } else if(faceAttributes.sadness > 0.1 && faceAttributes.sadness < 0.5) {
      app.WebAvatar.message("Are you sad today?");
    }
  }
};

app.createAvatar = function() {
  SDK.applicationId = "3683203999682257732";
  var sdk = new SDKConnection();
  var web = new WebAvatar();
  web.connection = sdk;
  web.avatar = "11785382";
  web.voice = "cmu-slt";
  web.speak = true;
  web.voiceMod = "default";
  web.width = "330";
  web.createBox();
  app.WebAvatar = web;
  app.WebAvatar.message("Welcome to GSK");
  // Re-Parent avatar
  var avatarDiv = document.getElementById("avatar-avatarbox");
  var newParent = document.getElementById("avatar-wrapper");
  newParent.appendChild(avatarDiv);
};

app.init = () => {
  console.log("application loaded");
  // Initialize camera
  app.initCam();

  app.createAvatar();
};

window.onload = function() {
  app.init();
};
