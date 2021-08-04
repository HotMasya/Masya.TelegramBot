import { AppBar, createStyles, IconButton, Theme, Toolbar, Typography, useMediaQuery, useTheme, withStyles } from '@material-ui/core';
import React from 'react';
import { Menu } from '@material-ui/icons';

export interface HeaderProps {
    onMenuClick: (event: React.MouseEvent) => void;
}

export default function Header(props: HeaderProps) {
    const theme = useTheme();
    const breakpoint = theme.breakpoints.down('sm');
    const isDownMd = useMediaQuery(breakpoint);
    const { onMenuClick } = props;

    return (
        <AppBar color="primary" position="static">
            <Toolbar>
                {isDownMd && <IconButton onClick={onMenuClick} ><Menu /></IconButton>}
            </Toolbar>
        </AppBar>
    )
}
