import React from "react";
import { Tooltip, TooltipProps } from "reactstrap";

export interface HipTooltipProps extends TooltipProps {}

export const HipTooltip = ({
  children,
  placement = "left",
  target,
  autohide = false,
  ...deferred
}: HipTooltipProps) => {
  const [isOpen, setIsOpen] = React.useState(false);
  return (
    <Tooltip
      isOpen={isOpen}
      toggle={() => setIsOpen(true)} // @laholstege CHANGE THIS BEFORE MERGING TO MAIN
      className="hip-tooltip"
      placement={placement}
      target={target}
      autohide={false}
      {...deferred}
    >
      {children}
    </Tooltip>
  );
};
