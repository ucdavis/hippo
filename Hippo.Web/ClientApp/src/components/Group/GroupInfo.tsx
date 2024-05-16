import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { ShowFor } from "../../Shared/ShowFor";
import { UsePermissions } from "../../Shared/UsePermissions";
import { GroupModel } from "../../types";
import { Button, CardSubtitle, CardText } from "reactstrap";
import GroupDetails from "./GroupDetails";

export interface GroupInfoProps {
  group: GroupModel;
  showDetails: () => void;
}

export const GroupInfo = ({ group, showDetails }: GroupInfoProps) => {
  const { canViewGroup } = UsePermissions();

  const handleShowDetails = async () => {
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
          <CardText>
            {group.admins?.map((a, i) => (
              <span key={i}>
                {a.name} ({a.email})<br />
              </span>
            ))}
          </CardText>
        </>
      )}
      <ShowFor condition={() => canViewGroup(group.name)}>
        <Button size="sm" onClick={handleShowDetails}>
          Details
        </Button>
      </ShowFor>
    </div>
  );
};
