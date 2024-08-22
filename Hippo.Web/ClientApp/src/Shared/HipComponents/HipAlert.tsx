import React from "react";
import { Alert, AlertProps } from "reactstrap";

interface HipAlertProps extends AlertProps {}

const HipAlert: React.FC<HipAlertProps> = ({ children, ...deferred }) => {
  return (
    <Alert className="hip-alert" {...deferred}>
      {children}
    </Alert>
  );
};

export default HipAlert;
