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
      toggle={() => setIsOpen(!isOpen)}
      className="hip-tooltip"
      style={{ backgroundColor: "rgb(233, 226, 237)" }}
      placement={placement}
      target={target}
      autohide={false}
      {...deferred}
    >
      {children}
    </Tooltip>
  );
};
