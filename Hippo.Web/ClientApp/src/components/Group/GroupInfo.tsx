import { ShowFor } from "../../Shared/ShowFor";
import { usePermissions } from "../../Shared/usePermissions";
import { GroupModel } from "../../types";
import { Button, CardSubtitle, CardText } from "reactstrap";
import { MouseEvent } from "react";

export interface GroupInfoProps {
  group: GroupModel;
  showDetails?: () => void;
}

export const GroupInfo = ({ group, showDetails }: GroupInfoProps) => {
  const { canViewGroup } = usePermissions();

  const handleShowDetails = async (e: MouseEvent<HTMLButtonElement>) => {
    e.stopPropagation();
    e.preventDefault();
    showDetails();
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
    </div>
  );
};
