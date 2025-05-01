import { ShowFor } from "../../Shared/ShowFor";
import { usePermissions } from "../../Shared/usePermissions";
import { GroupModel } from "../../types";
import { Button, CardSubtitle, CardText } from "reactstrap";
import { MouseEvent } from "react";

export interface GroupInfoProps {
  group: GroupModel;
  showDetails?: () => void;
  navigateToGroupMembers?: () => void;
}

export const GroupInfo = ({
  group,
  showDetails,
  navigateToGroupMembers,
}: GroupInfoProps) => {
  const { canViewGroup, canManageGroup } = usePermissions();

  const handleShowDetails = async (e: MouseEvent<HTMLButtonElement>) => {
    e.stopPropagation();
    e.preventDefault();
    showDetails();
  };

  const handleShowMembers = async (e: MouseEvent<HTMLButtonElement>) => {
    e.stopPropagation();
    e.preventDefault();
    navigateToGroupMembers();
  };

  return (
    <div className="group-info-baseline" key={group.id}>
      <p className="group-info-header">
        {group.displayName}{" "}
        {group.name !== group.displayName && `(${group.name})`}
      </p>
      {group.admins?.length > 0 && (
        <>
          <CardSubtitle>Group Sponsors/Admins:</CardSubtitle>
          <CardText className="mb-0">
            {group.admins?.map((a, i) => (
              <span key={i}>
                {a.name} ({a.email})<br />
              </span>
            ))}
          </CardText>
        </>
      )}
      <ShowFor condition={() => !!showDetails && canViewGroup(group.name)}>
        <Button size="sm" color="link" onClick={handleShowDetails}>
          Details
        </Button>
      </ShowFor>
      <ShowFor
        condition={() => !!navigateToGroupMembers && canManageGroup(group.name)}
      >
        {" - "}
        <Button size="sm" color="link" onClick={handleShowMembers}>
          View Members
        </Button>
        {group.revokedOn && (
          <>
            {" - "}
            <strong>Revoked on {new Date(group.revokedOn).toLocaleDateString()}</strong>
          </>
        )}
      </ShowFor>
    </div>
  );
};
