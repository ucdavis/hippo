import React from "react";
import HipMainWrapper from "../Layout/HipMainWrapper";
import HipTitle from "../Layout/HipTitle";
import HipBody from "../Layout/HipBody";

const NotAuthorized: React.FC = () => {
  return (
    <HipMainWrapper>
      <HipTitle title="Not Authorized" />
      <HipBody>
        <p>Sorry, you don't have access to this page</p>
      </HipBody>
    </HipMainWrapper>
  );
};

export default NotAuthorized;
