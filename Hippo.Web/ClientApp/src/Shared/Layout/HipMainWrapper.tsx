import React from "react";
import HipClientError from "../LoadingAndErrors/HipClientError";
import HipErrorBoundary from "../LoadingAndErrors/HipErrorBoundary";

interface HipMainProps {
  children: React.ReactNode;
}

const HipMainWrapper: React.FC<HipMainProps> = ({ children }) => {
  return (
    <div className="main">
      <HipErrorBoundary
        fallback={
          <HipClientError thereWasAnErrorLoadingThe="page" type="alert" />
        }
      >
        {children}
      </HipErrorBoundary>
    </div>
  );
};

export default HipMainWrapper;
