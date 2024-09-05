import React from "react";
import { UpdateOrderStatusModel } from "./status";
import StatusBar from "./StatusBar";
import StatusCard from "./StatusCard";

interface StatusDialogProps extends UpdateOrderStatusModel {
  isAdmin: boolean;
}

const StatusDialog: React.FC<StatusDialogProps> = ({
  newStatus,
  currentStatus,
  isAdmin,
}) => {
  return (
    <div>
      <StatusBar
        isAdmin={isAdmin}
        status={currentStatus}
        showOnHover={newStatus}
        hideTooltip={true}
      />
      <h3>
        You are changing the status of this order from{" "}
        <span className="hip-text-primary">{currentStatus}</span> to{" "}
        <span className="hip-text-primary">{newStatus}</span>.
      </h3>
      <br />
      <StatusCard
        status={currentStatus}
        isAdmin={isAdmin}
        showStatusActions={false}
      />
      <StatusCard
        status={newStatus}
        isAdmin={isAdmin}
        showStatusActions={true}
      />
    </div>
  );
};

export default StatusDialog;
