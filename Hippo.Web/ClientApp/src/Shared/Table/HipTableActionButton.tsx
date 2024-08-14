import React from "react";
import HipButton from "../HipButton";
import { ButtonProps } from "reactstrap";

interface HipTableActionButtonProps extends ButtonProps {
  children: React.ReactNode;
}

const HipTableActionButton: React.FC<HipTableActionButtonProps> = ({
  children,
  ...deferred
}) => {
  return (
    <div className="data-table-prolog float-end">
      <HipButton color="link" {...deferred}>
        {children}
      </HipButton>
    </div>
  );
};

export default HipTableActionButton;
