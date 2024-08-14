import React from "react";
import { TooltipProps, UncontrolledTooltip } from "reactstrap";

interface HipTooltipProps extends TooltipProps {}

export const HipTooltip = ({
  children,
  placement = "left",
  target,
  autohide = false,
  ...deferred
}: HipTooltipProps) => {
  return (
    <UncontrolledTooltip
      style={{ backgroundColor: "rgb(233, 226, 237)" }}
      placement={placement}
      target={target}
      autohide={false}
      {...deferred}
    >
      {children}
    </UncontrolledTooltip>
  );
};
