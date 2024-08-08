import React from "react";

interface HipBodyProps {
  children: React.ReactNode;
}

const HipBody: React.FC<HipBodyProps> = ({ children }) => {
  return <div className="hip-body justify-content-center">{children}</div>;
};

export default HipBody;
