import React from "react";

interface HipLoadingProps {
  message?: string;
}

const HipLoading: React.FC<HipLoadingProps> = ({ message }) => {
  return <div>{message ?? "Loading..."}</div>;
};

export default HipLoading;
