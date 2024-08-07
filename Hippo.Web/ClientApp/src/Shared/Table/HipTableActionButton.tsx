import React from "react";
import HipButton from "../HipButton";
import { ButtonProps } from "reactstrap";

interface HipTableActionButtonProps extends ButtonProps {}

const HipTableActionButton: React.FC<HipTableActionButtonProps> = ({
  ...deferred
}) => {
  return (
    <div className="data-table-prolog float-end">
      <HipButton color="link" {...deferred}>
        Create New Cluster
      </HipButton>
    </div>
  );
};

export default HipTableActionButton;
