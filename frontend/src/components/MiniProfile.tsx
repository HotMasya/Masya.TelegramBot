import { Avatar, Button, Typography, useTheme } from '@material-ui/core';
import React from 'react';
import CenteredBox from './containers/CenteredBox';

export type MiniProfileProps = {
  firstName: string;
  lastName?: string;
  avatar?: string;
  onClick?: (event: React.MouseEvent) => void;
};

const MiniProfile: React.FC<MiniProfileProps> = (props) => {
  const { firstName, lastName, avatar, children, onClick } = props;
  const fullName = lastName ? `${firstName} ${lastName}` : firstName;
  const theme = useTheme();
  return (
    <CenteredBox>
      <Button onClick={onClick} style={{ textTransform: 'none' }}>
        {avatar && <Avatar src={avatar} alt={fullName} />}
        <Typography variant="h5" style={{ marginLeft: theme.spacing(2) }}>
          {fullName}
        </Typography>
        {children}
      </Button>
    </CenteredBox>
  );
};

export default MiniProfile;
