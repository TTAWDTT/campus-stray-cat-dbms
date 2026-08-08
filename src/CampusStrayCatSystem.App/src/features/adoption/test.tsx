import type { CatSummary } from '../../types/cats'

/** 设为 true 则使用 Mock 数据，false 则调用真实接口 */
export const USE_MOCK = true

export const MOCK_CATS: CatSummary[] = [
  {
    catID: 'mock-001',
    catName: '芝麻',
    gender: 'FEMALE',
    breed: '中华田园猫',
    colorPattern: '黑白',
    sterilizedFlag: 1,
    earTipFlag: 1,
    personalityTags: '亲人，胆小',
    mainAreaId: 'area-01',
    mainAreaName: '图书馆周边',
    lifeStatus: 'ON_CAMPUS',
    archiveStatus: 'PUBLISHED',
    primaryPhotoUrl: null,
  },
  {
    catID: 'mock-002',
    catName: '橘子',
    gender: 'MALE',
    breed: '橘猫',
    colorPattern: '橘白',
    sterilizedFlag: 0,
    earTipFlag: 0,
    personalityTags: '贪吃，爱叫',
    mainAreaId: 'area-02',
    mainAreaName: '食堂附近',
    lifeStatus: 'ON_CAMPUS',
    archiveStatus: 'PUBLISHED',
    primaryPhotoUrl: null,
  },
  {
    catID: 'mock-003',
    catName: '花花',
    gender: 'FEMALE',
    breed: '三花猫',
    colorPattern: '三花',
    sterilizedFlag: 1,
    earTipFlag: 1,
    personalityTags: '独立，高冷',
    mainAreaId: 'area-03',
    mainAreaName: '宿舍区',
    lifeStatus: 'ON_CAMPUS',
    archiveStatus: 'PUBLISHED',
    primaryPhotoUrl: null,
  },
]
