import React from "react";

const NotFound: React.FC = () => {
  return (
    <div className="row justify-content-center">
      <div className="col-md-4">
        <h3 className="text-center">404 - Not Found</h3>
        <div style={{ position: "relative" }}>
          <img
            src="/media/not-found.jpg"
            className="img-fluid"
            alt="404 Not Found"
          />
          <h1
            style={{
              position: "absolute",
              top: "45%",
              left: 0,
              width: "100%",
              textAlign: "center",
              textShadow:
                "#000 0px 0 20px, #000 0px 0 20px, #000 0px 0 20px, #000 0px 0 20px",
              color: "#fff",
            }}
          >
            There's nothing to see here.
          </h1>
        </div>
      </div>
    </div>
  );
};

export default NotFound;
