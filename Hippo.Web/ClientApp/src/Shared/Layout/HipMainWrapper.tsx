import React from "react";

interface HipMainProps {
  children: React.ReactNode;
}

const HipMainWrapper: React.FC<HipMainProps> = ({ children }) => {
  return <div className="main">{children}</div>;
};

export default HipMainWrapper;
