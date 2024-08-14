import { faHippo } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

interface HipLoadingProps {}

const HipLoading: React.FC<HipLoadingProps> = () => {
  return (
    <div>
      Loading ...
      <FontAwesomeIcon icon={faHippo} bounce />
    </div>
  );
};

export default HipLoading;
