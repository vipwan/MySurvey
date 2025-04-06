import React, { useState, useEffect } from 'react';
import { Table, Typography, Badge, Spin, Empty, Tooltip, Timeline, Card, Divider } from 'antd';
import { ClockCircleOutlined, EditOutlined, DeleteOutlined, PlusOutlined, EyeOutlined, UserOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';

const { Title, Text } = Typography;

// 格式化日期时间
const formatDateTime = (dateString) => {
    if (!dateString) return '-';
    const date = dayjs(dateString);
    return date.isValid() ? date.format('YYYY-MM-DD HH:mm:ss') : '-';
};

// 操作类型映射到图标和颜色
const actionIconMap = {
    '创建内容': { icon: <PlusOutlined />, color: 'green' },
    '更新内容': { icon: <EditOutlined />, color: 'blue' },
    '删除内容': { icon: <DeleteOutlined />, color: 'red' },
    '状态变更': { icon: <EyeOutlined />, color: 'purple' },
    'View': { icon: <EyeOutlined />, color: 'cyan' },
    'Default': { icon: <ClockCircleOutlined />, color: 'grey' }
};

// 获取操作类型对应的图标和颜色
const getActionIcon = (action) => {
    return actionIconMap[action] || actionIconMap.Default;
};

const AuditLog = ({ contentId, api }) => {
    const [loading, setLoading] = useState(false);
    const [auditLogs, setAuditLogs] = useState([]);
    const [error, setError] = useState(null);

    // 获取审计日志数据
    useEffect(() => {
        if (!contentId) return;

        const fetchAuditLogs = async () => {
            setLoading(true);
            setError(null);
            try {
                // 使用后端API地址获取审计日志
                const response = await api.getAuditLogs(contentId);
                setAuditLogs(Array.isArray(response.data) ? response.data : []);
            } catch (err) {
                console.error('获取审计日志失败:', err);
                setError('获取审计日志失败，请稍后重试');
                setAuditLogs([]);
            } finally {
                setLoading(false);
            }
        };

        fetchAuditLogs();
    }, [contentId, api]);

    // 表格列配置
    const columns = [
        {
            title: '操作类型',
            dataIndex: 'action',
            key: 'action',
            width: 120,
            render: (action) => {
                const { icon, color } = getActionIcon(action);
                return (
                    <Badge color={color} text={
                        <span>
                            {icon} {action}
                        </span>
                    } />
                );
            },
        },
        {
            title: '详情',
            dataIndex: 'details',
            key: 'details',
            ellipsis: true,
        },
        {
            title: '操作者',
            dataIndex: 'operatorName',
            key: 'operatorName',
            width: 100,
            render: (text) => text || '-',
        },
        {
            title: '操作时间',
            dataIndex: 'timestamp',
            key: 'timestamp',
            width: 180,
            render: (text) => formatDateTime(text),
            sorter: (a, b) => new Date(a.timestamp) - new Date(b.timestamp),
            defaultSortOrder: 'descend',
        },
    ];

    // 时间线视图
    const renderTimeline = () => {
        if (auditLogs.length === 0) {
            return <Empty description="暂无审计日志" />;
        }

        return (
            <Timeline mode="left">
                {auditLogs.map((log) => {
                    const { icon, color } = getActionIcon(log.action);
                    return (
                        <Timeline.Item
                            key={log.id}
                            dot={icon}
                            color={color}
                            label={formatDateTime(log.timestamp)}
                        >
                            <div style={{ marginBottom: 8 }}>
                                <Text strong>{log.action}</Text>
                                {log.operatorId && (
                                    <Tooltip title="操作者">
                                        <Text type="secondary" style={{ marginLeft: 8 }}>
                                            <UserOutlined /> {log.operatorName}
                                        </Text>
                                    </Tooltip>
                                )}
                            </div>
                            <Text>{log.details}</Text>
                        </Timeline.Item>
                    );
                })}
            </Timeline>
        );
    };

    return (
        <Card
            title={
                <div style={{ display: 'flex', alignItems: 'center' }}>
                    <ClockCircleOutlined style={{ marginRight: 8 }} />
                    <span>审计日志</span>
                </div>
            }
            bordered
        >
            {loading ? (
                <div style={{ textAlign: 'center', padding: '40px 0' }}>
                    <Spin tip="加载审计日志中..." />
                </div>
            ) : error ? (
                <div style={{ textAlign: 'center', padding: '20px 0', color: '#ff4d4f' }}>
                    <Text type="danger">{error}</Text>
                </div>
            ) : (
                <>
                    <div className="timeline-view">
                        {renderTimeline()}
                    </div>
                    <Divider orientation="left">表格视图</Divider>
                    <Table
                        columns={columns}
                        dataSource={auditLogs}
                        rowKey="id"
                        pagination={{
                            pageSize: 5,
                            showSizeChanger: true,
                            pageSizeOptions: ['5', '10', '20'],
                            showTotal: (total) => `共 ${total} 条记录`
                        }}
                        size="small"
                        scroll={{ x: 'max-content' }}
                    />
                </>
            )}
        </Card>
    );
};

export default AuditLog;
