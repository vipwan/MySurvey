// mysurvey.client/src/components/dashboard/Dashboard.jsx
import React from 'react';
import { Card, Row, Col, Statistic, Spin, Skeleton } from 'antd';
import { UserOutlined, FileTextOutlined, FormOutlined } from '@ant-design/icons';
import { Line } from '@ant-design/charts';
import { useRequest, useInterval, useReactive } from 'ahooks';
import { surveyApi } from '../../services/api';

const Dashboard = () => {

    // 生成模拟的24小时在线用户数据
    const generateHourlyData = () => {
        const data = [];
        for (let i = 0; i < 24; i++) {
            // 生成随机用户数，但保持一定的趋势性
            // 早晨和晚上的用户数较少，下午和晚上的用户数较多
            let baseValue = 200;
            if (i >= 7 && i <= 11) {
                // 早晨到中午逐渐增加
                baseValue = 200 + (i - 7) * 50;
            } else if (i >= 12 && i <= 18) {
                // 下午到晚上保持高峰
                baseValue = 400;
            } else if (i >= 19 && i <= 22) {
                // 晚上逐渐减少
                baseValue = 350 - (i - 19) * 50;
            }

            // 添加一些随机波动
            const randomValue = Math.floor(Math.random() * 50);
            const value = baseValue + randomValue;

            data.push({
                hour: `${i}:00`,
                value: value,
                category: '在线用户',
            });
        }
        return data;
    };

    const hourlyData = useReactive({ data: generateHourlyData() });

    // 获取问卷统计数据
    const { data: surveyData, loading: surveyLoading } = useRequest(
        async () => {  
            const response = await surveyApi.stat();
            return response.data;  
        },
        {
            refreshDeps: [],
        }
    );

    // 线图配置
    const lineConfig = {
        data: hourlyData.data,
        xField: 'hour',
        yField: 'value',
        seriesField: 'category',
        smooth: true, // 平滑曲线
        xAxis: {
            title: {
                text: '时间',
            },
        },
        yAxis: {
            title: {
                text: '用户数',
            },
        },
        color: '#1890ff',
        point: {
            size: 4,
            shape: 'circle',
            style: {
                stroke: '#fff',
                lineWidth: 2,
            },
        },
        tooltip: {
            showMarkers: true,
        },
        state: {
            active: {
                style: {
                    shadowBlur: 4,
                    stroke: '#000',
                    fill: 'red',
                },
            },
        },
        interactions: [
            {
                type: 'marker-active',
            },
        ],
    };

    // 每隔一段时间更新数据，模拟实时更新
    useInterval(() => {
        hourlyData.data = generateHourlyData();
    }, 5000);//5秒更新
   
    if (surveyLoading) {
        return (
            <>
                <Skeleton />
                <div style={{ textAlign: 'center', padding: '50px' }}>
                    <Spin size="large">
                        <div style={{ padding: '30px' }}>加载中...</div>
                    </Spin>
                </div>
            </>
        );
    }

    return (
        <div className="dashboard">
            <Row gutter={[16, 16]}>
                <Col span={6}>
                    <Card>
                        <Statistic
                            title="总问卷数"
                            value={surveyData?.surveyCount || 0}
                            prefix={<FileTextOutlined />}
                        />
                    </Card>
                </Col>
                <Col span={6}>
                    <Card>
                        <Statistic
                            title="已完成问卷"
                            value={surveyData?.surveyCompleteCount || 0}
                            prefix={<FormOutlined />}
                        />
                    </Card>
                </Col>
                <Col span={6}>
                    <Card>
                        <Statistic
                            title="总回答数"
                            value={surveyData?.answerCount || 0}
                            prefix={<FileTextOutlined />}
                        />
                    </Card>
                </Col>
                <Col span={6}>
                    <Card>
                        <Statistic
                            title="活跃用户"
                            value={surveyData?.userCount || 0}
                            prefix={<UserOutlined />}
                        />
                    </Card>
                </Col>
            </Row>

            <Card title="24小时在线用户统计" style={{ marginTop: 16 }}>
                <div style={{ height: 200 }}>
                    <Line {...lineConfig} />
                </div>
            </Card>
        </div>
    );
};

export default Dashboard;
